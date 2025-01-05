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

namespace Gurux.DLMS.Simulator.Net
{
    internal class Settings
    {
        public readonly GXDLMSSecureClient Client = new(true);
        public const TraceLevel Trace = TraceLevel.Verbose;
        public IGXMedia? Media;
        public int ServerCount = 1;
        public string? InputFile;
        
        public static void InitConfiguration(string[] args, Settings settings)
        {
            settings.Client.UseLogicalNameReferencing = true;
            
            var config = GetConfig();

            ArgumentNullException.ThrowIfNull(config.Portnumber);
   
            settings.Media ??= new GXNet();
            var net = settings.Media as GXNet;
            net!.Port = config.Portnumber.Value;

            if (!string.IsNullOrEmpty(config.Password))
            {
                settings.Client.Password = Encoding.ASCII.GetBytes(config.Password);
            }
            
            ArgumentException.ThrowIfNullOrWhiteSpace(config.Interface);
            try
            {
                settings.Client.InterfaceType = (InterfaceType)Enum.Parse(typeof(InterfaceType), config.Interface);
            }
            catch (Exception)
            {
                throw new ArgumentException("Invalid interface type option. (HDLC, WRAPPER)");
            }
            
            ArgumentException.ThrowIfNullOrWhiteSpace(config.TemplateFile); 
            settings.InputFile = config.TemplateFile;
                
            settings.ServerCount = config.ServerCount;
        }

        private static MeterConfig GetConfig()
        {
            return new MeterConfig
            {
                Portnumber = 1000,
                ServerCount = 10,
                TemplateFile = "./hdlc_meter_template.xml",
                Interface = "HDLC",
                // TemplateFile = "./wrapper_meter_template.xml"
                // Interface = "WRAPPER",
                // Password = "foobaz"
            };
        }
    }
}
