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
        public static readonly InitFailedHandler VoidFailHandler = (InitFailedHandler) ((mgr, step) => false);

        /// <summary>
        /// Handler asks user through Console whether to repeat the step and then continues or just stops
        /// </summary>
        public static readonly InitFailedHandler FeedbackRepeatFailHandler = (InitFailedHandler) ((mgr, step) =>
        {
            InitMgr.s_log.Error(
                "\n\nInitialization Step failed - Do you want to repeat and continue (y) [or cancel startup (n)]? (y/n)");
            string str = Console.ReadLine();
            if (str != null && str.StartsWith("y", StringComparison.InvariantCultureIgnoreCase))
                return mgr.Execute(step);
            return false;
        });

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
            initMgr.InitDependencies((IEnumerable<DependentInitializationStep>) dependentInitors);
            return initMgr.PerformInitialization();
        }

        public InitMgr()
            : this(true, InitMgr.VoidFailHandler)
        {
            this.failHandler = InitMgr.VoidFailHandler;
        }

        public InitMgr(InitFailedHandler failHandler)
            : this(true, failHandler)
        {
            this.failHandler = failHandler;
        }

        public InitMgr(bool measureSteps, InitFailedHandler failHandler)
        {
            this.m_MeasureSteps = measureSteps;
            this.failHandler = failHandler;
            this.InitSteps = new Dictionary<InitializationPass, List<InitializationStep>>();
            this.Init();
        }

        public bool MeasureSteps
        {
            get { return this.m_MeasureSteps; }
            set { this.m_MeasureSteps = value; }
        }

        /// <summary>
        /// The <see cref="T:WCell.Core.Initialization.InitializationPass" /> that is currently being executed.
        /// </summary>
        public InitializationPass CurrentPass
        {
            get { return this.m_currentPass; }
        }

        public int GetStepCount(InitializationPass pass)
        {
            return this.InitSteps[pass].Count;
        }

        /// <summary>
        /// Finds, reads, and stores all initialization steps to be completed.
        /// </summary>
        private void Init()
        {
            foreach (InitializationPass key in Enum.GetValues(typeof(InitializationPass)))
            {
                if (!this.InitSteps.ContainsKey(key))
                    this.InitSteps.Add(key, new List<InitializationStep>());
            }
        }

        /// <summary>Adds all InitializationSteps of the given Assembly.</summary>
        /// <param name="asm"></param>
        public void AddStepsOfAsm(Assembly asm)
        {
            List<DependentInitializationStep> dependentInitors = new List<DependentInitializationStep>();
            foreach (Type type in asm.GetTypes())
                this.AddStepsOfType(type, dependentInitors);
            this.InitDependencies((IEnumerable<DependentInitializationStep>) dependentInitors);
        }

        public void AddGlobalMgrsOfAsm(Assembly asm)
        {
            foreach (Type type in asm.GetTypes())
            {
                if (((IEnumerable<GlobalMgrAttribute>) type.GetCustomAttributes<GlobalMgrAttribute>())
                    .FirstOrDefault<GlobalMgrAttribute>() != null)
                    this.UnresolvedDependencies.Add(type, new GlobalMgrInfo());
            }
        }

        public GlobalMgrInfo GetGlobalMgrInfo(Type t)
        {
            GlobalMgrInfo globalMgrInfo;
            this.UnresolvedDependencies.TryGetValue(t, out globalMgrInfo);
            return globalMgrInfo;
        }

        private void InitDependencies(IEnumerable<DependentInitializationStep> dependentSteps)
        {
            foreach (DependentInitializationStep dependentStep in dependentSteps)
            {
                foreach (InitializationDependency initializationDependency in dependentStep.Dependency)
                {
                    GlobalMgrInfo globalMgrInfo = this.GetGlobalMgrInfo(initializationDependency.DependentType);
                    if (globalMgrInfo == null)
                        throw new InitializationException(
                            "Invalid Dependency - {0} is dependent on {1} which is not a GlobalMgr.", new object[2]
                            {
                                (object) dependentStep.Step.InitMethod.GetFullMemberName(),
                                (object) initializationDependency.DependentType.FullName
                            });
                    globalMgrInfo.Dependencies.Add(dependentStep);
                    initializationDependency.DependentMgr = globalMgrInfo;
                }

                this.TryResolve(dependentStep);
            }
        }

        public void AddStepsOfType(Type type, List<DependentInitializationStep> dependentInitors)
        {
            if (((IEnumerable<GlobalMgrAttribute>) type.GetCustomAttributes<GlobalMgrAttribute>())
                .FirstOrDefault<GlobalMgrAttribute>() != null)
                this.UnresolvedDependencies.Add(type, new GlobalMgrInfo());
            foreach (MethodInfo method in type.GetMethods(BindingFlags.Static | BindingFlags.Public |
                                                          BindingFlags.NonPublic))
            {
                InitializationAttribute initializationAttribute =
                    ((IEnumerable<InitializationAttribute>) method.GetCustomAttributes<InitializationAttribute>())
                    .FirstOrDefault<InitializationAttribute>();
                DependentInitializationAttribute[] customAttributes =
                    method.GetCustomAttributes<DependentInitializationAttribute>();
                if (initializationAttribute != null)
                {
                    InitializationStep step = new InitializationStep(initializationAttribute.Pass,
                        initializationAttribute.Name, initializationAttribute.IsRequired, method);
                    if (customAttributes.Length > 0)
                    {
                        DependentInitializationStep initializationStep = new DependentInitializationStep(step,
                            ((IEnumerable<DependentInitializationAttribute>) customAttributes)
                            .TransformArray<DependentInitializationAttribute, InitializationDependency>(
                                (Func<DependentInitializationAttribute, InitializationDependency>) (attr =>
                                    new InitializationDependency(attr))));
                        dependentInitors.Add(initializationStep);
                    }
                    else
                        this.AddIndipendentStep(step);

                    this.m_newSteps = true;
                }
                else if (customAttributes.Length > 0)
                    throw new InitializationException("Invalid {0} - Requires missing {1} for: {2}", new object[3]
                    {
                        (object) typeof(DependentInitializationAttribute).Name,
                        (object) typeof(InitializationAttribute).Name,
                        (object) method.GetFullMemberName()
                    });
            }
        }

        private void AddIndipendentStep(InitializationStep step)
        {
            ++this.totalStepCount;
            this.InitSteps[step.Pass].Add(step);
        }

        /// <summary>
        /// Tries to execute all initialization steps, and returns the initialization result,
        /// logging every failure and success.
        /// </summary>
        /// <returns>true if all initialization steps completed, false if a required step failed.</returns>
        public bool PerformInitialization()
        {
            this.m_newSteps = false;
            foreach (InitializationPass pass in Enum.GetValues(typeof(InitializationPass)))
            {
                if (this.GetStepCount(pass) > 0)
                {
                    this.m_currentPass = pass;
                    InitMgr.s_log.Info(string.Format(WCell_Core.InitPass, (object) this.m_currentPass));
                    if (!this.Execute(pass))
                        return false;
                }
            }

            InitMgr.s_log.Info(string.Format(WCell_Core.InitComplete, (object) this.totalSuccess,
                (object) this.totalFails));
            return true;
        }

        private bool Execute(InitializationPass pass)
        {
            foreach (InitializationStep step in this.InitSteps[pass].ToArray())
            {
                if (!step.Executed && !this.Execute(step))
                    return false;
            }

            while (this.m_newSteps)
            {
                this.m_newSteps = false;
                for (InitializationPass pass1 = InitializationPass.First; pass1 <= this.m_currentPass; ++pass1)
                    this.Execute(pass1);
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
                object obj = step.InitMethod.Invoke((object) null, args);
                if (obj is bool && !(bool) obj)
                    InitMgr.s_log.Error(WCell_Core.InitStepFailed, (object) step.InitStepName,
                        (object) step.InitMethod.Name, (object) ".");
                else
                    flag = true;
            }
            catch (Exception ex)
            {
                LogUtil.ErrorException(ex, WCell_Core.InitStepFailed, (object) step.InitStepName,
                    (object) step.InitMethod.Name, (object) "");
            }

            if (flag)
            {
                ++this.totalSuccess;
                if (!string.IsNullOrEmpty(step.InitStepName))
                {
                    TimeSpan timeSpan = DateTime.Now - now;
                    if (!this.m_MeasureSteps)
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
                    InitMgr.s_log.Info(string.Format(WCell_Core.InitStepSucceeded, (object) step.InitStepName,
                        (object) str3));
                }
            }
            else if (!this.failHandler(this, step))
            {
                if (step.IsRequired)
                {
                    InitMgr.s_log.Fatal(string.Format(WCell_Core.InitStepWasRequired, (object) step.InitStepName,
                        (object) step.InitMethod.Name));
                    return false;
                }

                ++this.totalFails;
            }

            return true;
        }

        public void SignalGlobalMgrReady(Type type)
        {
            GlobalMgrInfo globalMgrInfo = this.GetGlobalMgrInfo(type);
            if (globalMgrInfo == null)
                throw new InitializationException(
                    "Invalid Signal - {0} signaled to be ready but did not register as a GlobalMgr.", new object[1]
                    {
                        (object) type.FullName
                    });
            if (globalMgrInfo.IsInitialized)
                return;
            globalMgrInfo.IsInitialized = true;
            foreach (DependentInitializationStep dependency in globalMgrInfo.Dependencies)
                this.TryResolve(dependency);
        }

        private void TryResolve(DependentInitializationStep depList)
        {
            if (!((IEnumerable<InitializationDependency>) depList.Dependency).All<InitializationDependency>(
                (Func<InitializationDependency, bool>) (dep => dep.DependentMgr.IsInitialized)))
                return;
            if (depList.Step.Pass != InitializationPass.Any && depList.Step.Pass > this.m_currentPass)
                this.AddIndipendentStep(depList.Step);
            else
                this.DoExecute(depList.Step);
        }

        private void DoExecute(InitializationStep step)
        {
            if (!this.Execute(step))
                throw new InitializationException("Failed to Execute dependent step: " + (object) step);
        }
    }
}