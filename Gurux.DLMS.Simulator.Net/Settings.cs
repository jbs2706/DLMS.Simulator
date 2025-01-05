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
            List<GXCmdParameter> parameters = GXCommon.GetParameters(args, "h:p:c:s:r:i:It:a:wP:g:S:C:n:v:o:T:A:B:D:d:l:F:r:x:N:Xx:G:f:ub:W:w:L:R");
            
            foreach (var it in parameters)
            {
                switch (it.Tag)
                {
                    case 'r':
                        if (string.Compare(it.Value, "sn", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            settings.Client.UseLogicalNameReferencing = false;
                        }
                        else if (string.Compare(it.Value, "ln", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            settings.Client.UseLogicalNameReferencing = true;
                        }
                        else
                        {
                            throw new ArgumentException("Invalid reference option.");
                        }
                        break;
                    case 'p': //Port.
                        settings.Media ??= new GXNet();
                        var net = settings.Media as GXNet;
                        net!.Port = int.Parse(it.Value);
                        break;
                    case 'P': //Password
                        settings.Client.Password = Encoding.ASCII.GetBytes(it.Value);
                        break;
                    case 'i':
                        try
                        {
                            settings.Client.InterfaceType = (InterfaceType)Enum.Parse(typeof(InterfaceType), it.Value);
                        }
                        catch (Exception)
                        {
                            throw new ArgumentException("Invalid interface type option. (HDLC, WRAPPER)");
                        }
                        break;
                    case 'x':
                        settings.InputFile = it.Value;
                        break;
                    case 'N':
                        settings.ServerCount = int.Parse(it.Value);
                        break;
                }
            }
        }
    }
}
