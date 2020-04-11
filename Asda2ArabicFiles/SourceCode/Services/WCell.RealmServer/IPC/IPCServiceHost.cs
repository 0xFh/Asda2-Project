/*************************************************************************
 *
 *   file		: ServiceHost.cs
 *   copyright		: (C) The WCell Team
 *   email		: info@wcell.org
 *   last changed	: $LastChangedDate: 2008-06-08 00:55:09 +0800 (Sun, 08 Jun 2008) $
 
 *   revision		: $Rev: 458 $
 *
 *   This program is free software; you can redistribute it and/or modify
 *   it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or
 *   (at your option) any later version.
 *
 *************************************************************************/

using System;
using System.ServiceModel;
using NLog;
using WCell.Intercommunication;

namespace WCell.RealmServer.IPC
{
	/// <summary>
	/// Defines a host with the authentication service.
	/// </summary>
	public static class IPCServiceHost
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();

		public static ServiceHost Host;

		public static bool IsOpen
		{
			get { return Host != null && Host.State == CommunicationState.Opened; }
		}

		/// <summary>
		/// Starts the authentication service
		/// </summary>
		public static void StartService()
		{
			if (!IsOpen)
			{
				StopService();		// make sure, there is no half-open connection pending

				lock (typeof(IPCServiceHost))
				{
					Host = new ServiceHost(typeof(IPCServiceAdapter));
					var binding = new NetTcpBinding();
					binding.Security.Mode = SecurityMode.None;
                    
					var endPoint = Host.AddServiceEndpoint(
						typeof(IWCellIntercomService),
						binding,
						RealmServerConfiguration.IPCAddress);
					Host.Open();

					var addr = Host.Description.Endpoints[0].ListenUri.AbsoluteUri;
					log.Info("IPC service started on {0}", addr);
				}
			}
		}

		/// <summary>
		/// Stops the service.
		/// </summary>
		public static void StopService()
		{
			lock (typeof(IPCServiceHost))
			{
				if (Host != null && Host.State != CommunicationState.Closed && Host.State != CommunicationState.Faulted)
				{
					try
					{
						Host.Close();
					}
					catch (Exception)
					{
						// do nada
					}
					log.Info("IPC service stoped.");
				}

				Host = null;
			}
		}
	}
}