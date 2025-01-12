//
// --------------------------------------------------------------------------
//  Gurux Ltd
//
//
//
// Filename:        $HeadURL$
//
// Version:         $Revision$,
//                  $Date$
//                  $Author$
//
// Copyright (c) Gurux Ltd
//
//---------------------------------------------------------------------------
//
//  DESCRIPTION
//
// This file is a part of Gurux Device Framework.
//
// Gurux Device Framework is Open Source software; you can redistribute it
// and/or modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; version 2 of the License.
// Gurux Device Framework is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU General Public License for more details.
//
// More information of Gurux products: http://www.gurux.org
//
// This code is licensed under the GNU General Public License v2.
// Full text may be retrieved at http://www.gnu.org/licenses/gpl-2.0.txt
//---------------------------------------------------------------------------

using Gurux.Common;
using Gurux.DLMS.Enums;
using Gurux.DLMS.Secure;
using Gurux.Net;
using System.Diagnostics;
using System.Text;

namespace Gurux.DLMS.Simulator.Net;

internal class Settings
{
    private const string PortnumberEnvName = "DLMS_SIMULATOR_PORT";
    private const string ServerCountEnvName = "DLMS_SIMULATOR_SERVER_COUNT";
    private const string InterfaceEnvName = "DLMS_SIMULATOR_INTERFACE";
    private const string PasswordEnvName = "DLMS_SIMULATOR_PASSWORD";
    
    public readonly GXDLMSSecureClient Client = new(true);
    public const TraceLevel Trace = TraceLevel.Verbose;
    public IGXMedia? Media;
    public int ServerCount = 1;
    public const string InputFile = "./dlms-meter-template.xml";

    public static Settings InitConfigurationAsync()
    {
        var settings = new Settings();
        
        if (!File.Exists("./dlms-meter-template.xml"))
        {
            throw new FileNotFoundException($"dlms-meter-template.xml not found in current dictionary {Directory.GetCurrentDirectory()}");
        }
        
        settings.Client.UseLogicalNameReferencing = true;
        
        var portnumber = Environment.GetEnvironmentVariable(PortnumberEnvName);
        ArgumentNullException.ThrowIfNull(portnumber);
   
        settings.Media ??= new GXNet();
        var net = settings.Media as GXNet;
        net!.Port = int.Parse(portnumber);

        var password = Environment.GetEnvironmentVariable(PasswordEnvName);
        
        if (!string.IsNullOrEmpty(password))
        {
            settings.Client.Password = Encoding.ASCII.GetBytes(password);
        }
            
        var dlmsInterface = Environment.GetEnvironmentVariable(InterfaceEnvName);
        ArgumentException.ThrowIfNullOrWhiteSpace(dlmsInterface);
        
        try
        {
            settings.Client.InterfaceType = (InterfaceType)Enum.Parse(typeof(InterfaceType), dlmsInterface);
        }
        catch (Exception)
        {
            throw new ArgumentException("Invalid interface type option. (HDLC, WRAPPER)");
        }
            
        var serverCount = Environment.GetEnvironmentVariable(ServerCountEnvName);
        ArgumentException.ThrowIfNullOrWhiteSpace(serverCount);
        
        settings.ServerCount = int.Parse(serverCount);

        return settings;
    }
}