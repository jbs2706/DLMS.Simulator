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

using Gurux.DLMS.Enums;
using Gurux.DLMS.Objects;
using Gurux.Net;
using Gurux.Serial;
using System.Text;

namespace Gurux.DLMS.Simulator.Net;

internal class Program
{
    private static int Main(string[] args)
    {
        try
        {
            var settings = Settings.InitConfigurationAsync();
            if (!string.IsNullOrEmpty(Settings.InputFile))
            {
                StartSimulator(settings);
            }
            else
            {
                Console.WriteLine("Device values file is not given.");
            }
        }
        catch (IOException ex)
        {
            Console.WriteLine("----------------------------------------------------------");
            Console.WriteLine(ex.Message);
            Console.WriteLine("Available ports:");
            Console.WriteLine(string.Join(" ", GXSerial.GetPortNames()));
            return 1;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return 1;
        }

        return 0;
    }

    /// <summary>
    /// Start simulator.
    /// </summary>
    private static void StartSimulator(Settings settings)
    {
        if (settings.Media is GXSerial)
        {
            var server = new GXDLMSMeter(settings.Client.UseLogicalNameReferencing, settings.Client.InterfaceType,
                settings.Client.UseUtc2NormalTime, settings.Client.ManufacturerId);
            if (settings.Client.UseLogicalNameReferencing)
            {
                Console.WriteLine("Logical Name DLMS Server in serial port {0} using {1}.", settings.Media,
                    settings.Client.InterfaceType);
            }
            else
            {
                Console.WriteLine("Short Name DLMS Server in serial port {0} using {1}.", settings.Media,
                    settings.Client.InterfaceType);
            }

            server.Initialize(settings.Media, Settings.Trace, Settings.InputFile, 1, false, null);
            UpdateSettings(settings, server);
            Console.WriteLine("----------------------------------------------------------");
        }
        else
        {
            //Create Network media component and start listen events.
            //4059 is Official DLMS port.
            ///////////////////////////////////////////////////////////////////////
            //Create Gurux DLMS server component for Short Name and start listen events.
            List<GXDLMSMeter> servers = [];
            var str = "DLMS " + settings.Client.InterfaceType;
            if (settings.Client.UseLogicalNameReferencing)
            {
                str += " Logical Name ";
            }
            else
            {
                str += " Short Name ";
            }

            var net = (GXNet)settings.Media;
            net.Server = true;
            Console.WriteLine(str + "simulator start in {0} ports {1}-{2}.", net.Protocol, net.Port,
                net.Port + settings.ServerCount - 1);

            var index = 0;
            GXDLMSObjectCollection? sharedObjects = null;
            for (var pos = 0; pos != settings.ServerCount; ++pos)
            {
                ++index;
                var server = new GXDLMSMeter(settings.Client.UseLogicalNameReferencing, settings.Client.InterfaceType,
                    settings.Client.UseUtc2NormalTime, settings.Client.ManufacturerId);
                servers.Add(server);

                try
                {
                    server.Initialize(new GXNet(net.Protocol, net.Port + pos), Settings.Trace,
                        Settings.InputFile, (UInt32)index + 1,
                        false, sharedObjects);
                }
                catch (System.Net.Sockets.SocketException ex)
                {
                    Console.WriteLine($"Port {net.Port + pos} already in use.");
                }

                UpdateSettings(settings, server);
                if (pos == 0 && settings.Client.UseLogicalNameReferencing)
                {
                    str = "Server address: " + settings.Client.ServerAddress.ToString();
                    Console.WriteLine(str);
                    Console.WriteLine("Associations:");
                    foreach (GXDLMSAssociationLogicalName it in server.Items.GetObjects(ObjectType
                                 .AssociationLogicalName))
                    {
                        str = "++++++++++++++++++++++++++++" + Environment.NewLine;
                        //Overwrite the password.
                        if (settings.Client.Password != null && settings.Client.Password.Length != 0)
                        {
                            it.Secret = settings.Client.Password;
                        }

                        str += "Client address: " + it.ClientSAP.ToString();
                        if (it.AuthenticationMechanismName.MechanismId == Authentication.None)
                        {
                            str += " Without authentication.";
                        }
                        else
                        {
                            str += $" {it.AuthenticationMechanismName.MechanismId} authentication";
                            if (it.Secret != null)
                            {
                                str += $", password {Encoding.ASCII.GetString(it.Secret)}";
                            }
                        }

                        str += Environment.NewLine + " Conformance:" + Environment.NewLine;
                        str += it.XDLMSContextInfo.Conformance + Environment.NewLine;
                        str += " MaxReceivePduSize: " + it.XDLMSContextInfo.MaxReceivePduSize;
                        str += " MaxSendPduSize: " + it.XDLMSContextInfo.MaxSendPduSize + Environment.NewLine;
                        var ss =
                            server.Items.FindByLN(ObjectType.SecuritySetup, it.SecuritySetupReference) as
                                GXDLMSSecuritySetup;
                        if (ss != null)
                        {
                            str += Environment.NewLine;
                            str += " Security suite: " + ss.SecuritySuite;
                            str += Environment.NewLine;
                            str += " Security policy: " + ss.SecurityPolicy;
                            str += Environment.NewLine;
                            str += " Authentication key: " + GXDLMSTranslator.ToHex(ss.Gak);
                            str += Environment.NewLine;
                            str += " Block cipher key: " + GXDLMSTranslator.ToHex(ss.Guek);
                            str += Environment.NewLine;
                            if (ss.Gbek != null)
                            {
                                str += " Broadcast block cipher key: " + GXDLMSTranslator.ToHex(ss.Gbek);
                            }

                            str += Environment.NewLine;
                        }

                        Console.WriteLine(str);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Password are given as command line parameters 
    /// because they can't read from the meter.
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="server"></param>
    private static void UpdateSettings(Settings settings, GXDLMSMeter server)
    {
        if (settings.Client.Password == null) return;
        
        foreach (var o in server.Items.GetObjects(ObjectType.AssociationLogicalName))
        {
            var it = (GXDLMSAssociationLogicalName)o;
            it.Secret = settings.Client.Password;
        }
    }
}