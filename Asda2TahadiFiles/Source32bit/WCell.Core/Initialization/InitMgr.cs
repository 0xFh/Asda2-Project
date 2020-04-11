using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WCell.Core.Localization;
using WCell.Util;
using WCell.Util.NLog;

namespace WCell.Core.Initialization
{
  /// <summary>
  /// Handles the loading and execution of all initialization code.
  /// </summary>
  public class InitMgr
  {
    private static readonly Logger s_log = LogManager.GetCurrentClassLogger();
    public static readonly InitFailedHandler VoidFailHandler = (mgr, step) => false;

    /// <summary>
    /// Handler asks user through Console whether to repeat the step and then continues or just stops
    /// </summary>
    public static readonly InitFailedHandler FeedbackRepeatFailHandler = (mgr, step) =>
    {
      s_log.Error(
        "\n\nInitialization Step failed - Do you want to repeat and continue (y) [or cancel startup (n)]? (y/n)");
      string str = Console.ReadLine();
      if(str != null && str.StartsWith("y", StringComparison.InvariantCultureIgnoreCase))
        return mgr.Execute(step);
      return false;
    };

    public readonly Dictionary<Type, GlobalMgrInfo> UnresolvedDependencies = new Dictionary<Type, GlobalMgrInfo>();
    public readonly Dictionary<InitializationPass, List<InitializationStep>> InitSteps;
    private int totalStepCount;
    private InitializationPass m_currentPass;
    private int totalFails;
    private int totalSuccess;
    private InitFailedHandler failHandler;
    private bool m_MeasureSteps;
    private bool m_newSteps;

    /// <summary>Initializes all Types of the given Assembly.</summary>
    /// <returns>Whether initialization could be performed for all found steps.</returns>
    public static bool Initialize(Assembly asm)
    {
      InitMgr initMgr = new InitMgr();
      initMgr.AddStepsOfAsm(asm);
      return initMgr.PerformInitialization();
    }

    /// <summary>Initializes the given Type.</summary>
    /// <returns>Whether initialization could be performed for all found steps in the given type.</returns>
    public static bool Initialize(Type type)
    {
      InitMgr initMgr = new InitMgr();
      List<DependentInitializationStep> dependentInitors = new List<DependentInitializationStep>();
      initMgr.AddStepsOfType(type, dependentInitors);
      initMgr.InitDependencies(dependentInitors);
      return initMgr.PerformInitialization();
    }

    public InitMgr()
      : this(true, VoidFailHandler)
    {
      failHandler = VoidFailHandler;
    }

    public InitMgr(InitFailedHandler failHandler)
      : this(true, failHandler)
    {
      this.failHandler = failHandler;
    }

    public InitMgr(bool measureSteps, InitFailedHandler failHandler)
    {
      m_MeasureSteps = measureSteps;
      this.failHandler = failHandler;
      InitSteps = new Dictionary<InitializationPass, List<InitializationStep>>();
      Init();
    }

    public bool MeasureSteps
    {
      get { return m_MeasureSteps; }
      set { m_MeasureSteps = value; }
    }

    /// <summary>
    /// The <see cref="T:WCell.Core.Initialization.InitializationPass" /> that is currently being executed.
    /// </summary>
    public InitializationPass CurrentPass
    {
      get { return m_currentPass; }
    }

    public int GetStepCount(InitializationPass pass)
    {
      return InitSteps[pass].Count;
    }

    /// <summary>
    /// Finds, reads, and stores all initialization steps to be completed.
    /// </summary>
    private void Init()
    {
      foreach(InitializationPass key in Enum.GetValues(typeof(InitializationPass)))
      {
        if(!InitSteps.ContainsKey(key))
          InitSteps.Add(key, new List<InitializationStep>());
      }
    }

    /// <summary>Adds all InitializationSteps of the given Assembly.</summary>
    /// <param name="asm"></param>
    public void AddStepsOfAsm(Assembly asm)
    {
      List<DependentInitializationStep> dependentInitors = new List<DependentInitializationStep>();
      foreach(Type type in asm.GetTypes())
        AddStepsOfType(type, dependentInitors);
      InitDependencies(dependentInitors);
    }

    public void AddGlobalMgrsOfAsm(Assembly asm)
    {
      foreach(Type type in asm.GetTypes())
      {
        if(type.GetCustomAttributes<GlobalMgrAttribute>()
             .FirstOrDefault() != null)
          UnresolvedDependencies.Add(type, new GlobalMgrInfo());
      }
    }

    public GlobalMgrInfo GetGlobalMgrInfo(Type t)
    {
      GlobalMgrInfo globalMgrInfo;
      UnresolvedDependencies.TryGetValue(t, out globalMgrInfo);
      return globalMgrInfo;
    }

    private void InitDependencies(IEnumerable<DependentInitializationStep> dependentSteps)
    {
      foreach(DependentInitializationStep dependentStep in dependentSteps)
      {
        foreach(InitializationDependency initializationDependency in dependentStep.Dependency)
        {
          GlobalMgrInfo globalMgrInfo = GetGlobalMgrInfo(initializationDependency.DependentType);
          if(globalMgrInfo == null)
            throw new InitializationException(
              "Invalid Dependency - {0} is dependent on {1} which is not a GlobalMgr.",
              (object) dependentStep.Step.InitMethod.GetFullMemberName(),
              (object) initializationDependency.DependentType.FullName);
          globalMgrInfo.Dependencies.Add(dependentStep);
          initializationDependency.DependentMgr = globalMgrInfo;
        }

        TryResolve(dependentStep);
      }
    }

    public void AddStepsOfType(Type type, List<DependentInitializationStep> dependentInitors)
    {
      if(type.GetCustomAttributes<GlobalMgrAttribute>()
           .FirstOrDefault() != null)
        UnresolvedDependencies.Add(type, new GlobalMgrInfo());
      foreach(MethodInfo method in type.GetMethods(BindingFlags.Static | BindingFlags.Public |
                                                   BindingFlags.NonPublic))
      {
        InitializationAttribute initializationAttribute =
          method.GetCustomAttributes<InitializationAttribute>()
            .FirstOrDefault();
        DependentInitializationAttribute[] customAttributes =
          method.GetCustomAttributes<DependentInitializationAttribute>();
        if(initializationAttribute != null)
        {
          InitializationStep step = new InitializationStep(initializationAttribute.Pass,
            initializationAttribute.Name, initializationAttribute.IsRequired, method);
          if(customAttributes.Length > 0)
          {
            DependentInitializationStep initializationStep = new DependentInitializationStep(step,
              customAttributes
                .TransformArray(
                  attr =>
                    new InitializationDependency(attr)));
            dependentInitors.Add(initializationStep);
          }
          else
            AddIndipendentStep(step);

          m_newSteps = true;
        }
        else if(customAttributes.Length > 0)
          throw new InitializationException("Invalid {0} - Requires missing {1} for: {2}",
            (object) typeof(DependentInitializationAttribute).Name, (object) typeof(InitializationAttribute).Name,
            (object) method.GetFullMemberName());
      }
    }

    private void AddIndipendentStep(InitializationStep step)
    {
      ++totalStepCount;
      InitSteps[step.Pass].Add(step);
    }

    /// <summary>
    /// Tries to execute all initialization steps, and returns the initialization result,
    /// logging every failure and success.
    /// </summary>
    /// <returns>true if all initialization steps completed, false if a required step failed.</returns>
    public bool PerformInitialization()
    {
      m_newSteps = false;
      foreach(InitializationPass pass in Enum.GetValues(typeof(InitializationPass)))
      {
        if(GetStepCount(pass) > 0)
        {
          m_currentPass = pass;
          s_log.Info(string.Format(WCell_Core.InitPass, m_currentPass));
          if(!Execute(pass))
            return false;
        }
      }

      s_log.Info(string.Format(WCell_Core.InitComplete, totalSuccess,
        totalFails));
      return true;
    }

    private bool Execute(InitializationPass pass)
    {
      foreach(InitializationStep step in InitSteps[pass].ToArray())
      {
        if(!step.Executed && !Execute(step))
          return false;
      }

      while(m_newSteps)
      {
        m_newSteps = false;
        for(InitializationPass pass1 = InitializationPass.First; pass1 <= m_currentPass; ++pass1)
          Execute(pass1);
      }

      return true;
    }

    public bool Execute(InitializationStep step)
    {
      step.Executed = true;
      bool flag = false;
      object[] args = step.GetArgs(this);
      DateTime now = DateTime.Now;
      try
      {
        object obj = step.InitMethod.Invoke(null, args);
        if(obj is bool && !(bool) obj)
          s_log.Error(WCell_Core.InitStepFailed, step.InitStepName,
            step.InitMethod.Name, ".");
        else
          flag = true;
      }
      catch(Exception ex)
      {
        LogUtil.ErrorException(ex, WCell_Core.InitStepFailed, (object) step.InitStepName,
          (object) step.InitMethod.Name, (object) "");
      }

      if(flag)
      {
        ++totalSuccess;
        if(!string.IsNullOrEmpty(step.InitStepName))
        {
          TimeSpan timeSpan = DateTime.Now - now;
          if(!m_MeasureSteps)
            ;
          string[] strArray1 = new string[5]
          {
            timeSpan.Minutes.ToString().PadLeft(2, '0'),
            ":",
            null,
            null,
            null
          };
          string[] strArray2 = strArray1;
          int index1 = 2;
          int num = timeSpan.Seconds;
          string str1 = num.ToString().PadLeft(2, '0');
          strArray2[index1] = str1;
          strArray1[3] = ".";
          string[] strArray3 = strArray1;
          int index2 = 4;
          num = timeSpan.Milliseconds;
          string str2 = num.ToString().PadLeft(2, '0');
          strArray3[index2] = str2;
          string str3 = string.Concat(strArray1);
          s_log.Info(string.Format(WCell_Core.InitStepSucceeded, step.InitStepName,
            str3));
        }
      }
      else if(!failHandler(this, step))
      {
        if(step.IsRequired)
        {
          s_log.Fatal(string.Format(WCell_Core.InitStepWasRequired, step.InitStepName,
            step.InitMethod.Name));
          return false;
        }

        ++totalFails;
      }

      return true;
    }

    public void SignalGlobalMgrReady(Type type)
    {
      GlobalMgrInfo globalMgrInfo = GetGlobalMgrInfo(type);
      if(globalMgrInfo == null)
        throw new InitializationException(
          "Invalid Signal - {0} signaled to be ready but did not register as a GlobalMgr.", (object) type.FullName);
      if(globalMgrInfo.IsInitialized)
        return;
      globalMgrInfo.IsInitialized = true;
      foreach(DependentInitializationStep dependency in globalMgrInfo.Dependencies)
        TryResolve(dependency);
    }

    private void TryResolve(DependentInitializationStep depList)
    {
      if(!depList.Dependency.All(
        dep => dep.DependentMgr.IsInitialized))
        return;
      if(depList.Step.Pass != InitializationPass.Any && depList.Step.Pass > m_currentPass)
        AddIndipendentStep(depList.Step);
      else
        DoExecute(depList.Step);
    }

    private void DoExecute(InitializationStep step)
    {
      if(!Execute(step))
        throw new InitializationException("Failed to Execute dependent step: " + step);
    }
  }
}