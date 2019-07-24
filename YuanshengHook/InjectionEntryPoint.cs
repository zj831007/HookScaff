using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Channels.Ipc;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Channels;
using System.Collections;
using System.Runtime.Serialization.Formatters;
using System.Security.Principal;
using System.IO;

namespace YuanshengHook
{
    //此类必须继承EasyHook.IEntryPoint ， 当对目标进程注入完成后，此类将变成目标进程的 EntryPoint
    public class InjectionEntryPoint : EasyHook.IEntryPoint
    {
        ServerInterface _serverInterface = null;
        CommandInterface _commandInterface = null;
        Queue<string> _messageQueue = new Queue<string>();



        public InjectionEntryPoint(EasyHook.RemoteHooking.IContext contex, string channelName)
        {
            /// IPC Client   客户端可以发消息到服务端
            /// ------------------------------------------------------
            _serverInterface = EasyHook.RemoteHooking.IpcConnectClient<ServerInterface>(channelName);
            _serverInterface.Ping();
            /// ------------------------------------------------------

            /// IPC Server  启动一个Server端，用于接入控制台发送的指令
            /// security identifiers (SIDs)
            /// ------------------------------------------------------
            WellKnownSidType[] InAllowedClientSIDs = new WellKnownSidType[] { WellKnownSidType.WorldSid };
            string channelNameDll = "CommandChannela";
            _commandInterface = new CommandInterface();

            EasyHook.RemoteHooking.IpcCreateServer(
                ref channelNameDll,
                System.Runtime.Remoting.WellKnownObjectMode.Singleton,
                _commandInterface,
                InAllowedClientSIDs
                );
            /// ------------------------------------------------------
        }

        public void DecriptAsset()
        {
            _serverInterface.ReportMessage("开始解密assetbundle。。。。");


            GetDecodeFileLen_Delegate FileLen_Delegate = (GetDecodeFileLen_Delegate)Marshal.GetDelegateForFunctionPointer(new IntPtr(0x7FF76FA5B8C0), typeof(GetDecodeFileLen_Delegate));
            DecodeBytes_Delegate decode_Delegate = (DecodeBytes_Delegate)Marshal.GetDelegateForFunctionPointer(new IntPtr(0x7FF76FA5C7B0), typeof(DecodeBytes_Delegate));

            string filePath = "E:/BaiduNetdiskDownload/yuanshen/GS_Data/StreamingAssets/AssetBundles/00/000b6059.asb";

            byte[] ab_bytes = File.ReadAllBytes(filePath);

            long fileLen = FileLen_Delegate(ab_bytes.LongLength); //计算解密后的长度


            byte[] decode_bytes = new byte[fileLen];

            decode_Delegate(ab_bytes, ab_bytes.LongLength, ref decode_bytes, fileLen, 0L, new IntPtr(0), -2L);


            _serverInterface.ReportMessage( "encode Len:" + ab_bytes.LongLength + ", decode len:"+ fileLen);
        }

        //此方法在目标进程执行
        public void Run(EasyHook.RemoteHooking.IContext contex, string channelName)
        {
            //注入完成，并且 server interface 已经连接
            _serverInterface.IsInstalled(EasyHook.RemoteHooking.GetCurrentProcessId());

            //接收控制台命令
            _commandInterface.DecryptEvent += DecriptAsset;


            EasyHook.RemoteHooking.WakeUpProcess();

            try
            {
                while (true)
                {
                    System.Threading.Thread.Sleep(500);

                    string[] queued = null;

                    lock (_messageQueue)
                    {
                        queued = _messageQueue.ToArray();
                        _messageQueue.Clear();
                    }

                    // Send newly monitored file accesses to FileMonitor
                    if (queued != null && queued.Length > 0)
                    {
                        _serverInterface.ReportMessages(queued);
                    }
                    else
                    {
                        _serverInterface.Ping();
                    }
                }
            }
            catch {
                 //如果host不可用， Ping() 或 ReportMessages() 将抛出异常
            }

            
            EasyHook.LocalHook.Release();
        }



        [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError =true)]
        delegate long GetDecodeFileLen_Delegate( long encodeFileLen );

        [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
        delegate int DecodeBytes_Delegate(byte[] bytes, long encodeLen ,ref  byte[] output ,long decodeLen,long xxx,IntPtr n , long  flag);



    }
}
