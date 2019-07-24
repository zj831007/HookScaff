using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Channels.Ipc;
using System.Text;

namespace Yuansheng
{
    class Program
    {
        

        static void Main(string[] args)
        {
            Int32 targetPID = 0;
            string targetExe = null;

            string channelName = null;

            ProcessArgs(args, out targetPID, out targetExe);

            if (targetPID <= 0 && string.IsNullOrEmpty(targetExe))
                return;


            /// IPC Server
            /// ------------------------------------------------------
            IpcServerChannel ipcServerChannel = EasyHook.RemoteHooking.IpcCreateServer<YuanshengHook.ServerInterface>(
                ref channelName, 
                System.Runtime.Remoting.WellKnownObjectMode.Singleton
                );
            /// ------------------------------------------------------


            string injectionLibrary = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "YuanshengHook.dll");

            try
            {
                // Injecting into existing process by Id
                if (targetPID > 0)
                {
                    Console.WriteLine("Attempting to inject into process {0}", targetPID);

                    // inject into existing process
                    EasyHook.RemoteHooking.Inject(
                        targetPID,          // ID of process to inject into
                        injectionLibrary,   // 32-bit library to inject (if target is 32-bit)
                        injectionLibrary,   // 64-bit library to inject (if target is 64-bit)
                        channelName         // the parameters to pass into injected library
                                            // ...
                    );
                }
                // Create a new process and then inject into it
                else if (!string.IsNullOrEmpty(targetExe))
                {
                    Console.WriteLine("Attempting to create and inject into {0}", targetExe);
                    // start and inject into a new process
                    EasyHook.RemoteHooking.CreateAndInject(
                        targetExe,          // executable to run
                        "",                 // command line arguments for target
                        0,                  // additional process creation flags to pass to CreateProcess
                        EasyHook.InjectionOptions.DoNotRequireStrongName, // allow injectionLibrary to be unsigned
                        injectionLibrary,   // 32-bit library to inject (if target is 32-bit)
                        injectionLibrary,   // 64-bit library to inject (if target is 64-bit)
                        out targetPID,      // retrieve the newly created process ID
                        channelName         // the parameters to pass into injected library
                                            // ...
                    );
                }
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("There was an error while injecting into target:");
                Console.ResetColor();
                Console.WriteLine(e.ToString());
            }


            Console.ForegroundColor = ConsoleColor.DarkGreen;
            while (true)
            {
                Console.WriteLine("==========================================");
                Console.WriteLine("请输入指令   e：(解密资源), d: (断开IPC连接), exit: (退出) \r\n");
                string commd = Console.ReadLine();

                if (commd.Equals("e"))
                {
                    /// IPC Client   客户端可以发消息到服务端
                    /// ------------------------------------------------------
                    YuanshengHook.CommandInterface _commandInterface = EasyHook.RemoteHooking.IpcConnectClient<YuanshengHook.CommandInterface>("CommandChannela");
                    _commandInterface.DoDecryptAB();
                   
                    /// ------------------------------------------------------

                }
                else
                {
                    break;
                }
           
            }

           
            Console.WriteLine("<Press any key to exit>");
            Console.ResetColor();
            Console.ReadKey();

        }

        static void ProcessArgs(string[] args, out int targetPID, out string targetExe)
        {
            targetPID = 0;
            targetExe = null;

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine(@"______ _ _     ___  ___            _ _             
|  ___(_) |    |  \/  |           (_) |            
| |_   _| | ___| .  . | ___  _ __  _| |_ ___  _ __ 
|  _| | | |/ _ \ |\/| |/ _ \| '_ \| | __/ _ \| '__|
| |   | | |  __/ |  | | (_) | | | | | || (_) | |   
\_|   |_|_|\___\_|  |_/\___/|_| |_|_|\__\___/|_|");

            Console.WriteLine("-------------------------------------------------------");
            Console.WriteLine("    调用目标进程中的函数   ");
            Console.WriteLine();
            Console.ResetColor();

            // Load any parameters
            while ((args.Length != 1) || !Int32.TryParse(args[0], out targetPID) || !File.Exists(args[0]))
            {
                if (targetPID > 0)
                {
                    break;
                }
                if (args.Length != 1 || !File.Exists(args[0]))
                {
                    Console.WriteLine("使用方法: 输入进程ID");
                    Console.WriteLine("      或: 可执行文件路径");
                    Console.WriteLine("");
                    Console.WriteLine("例如. :  1234");
                    Console.WriteLine("         用于注入进程 PID 1234");
                    Console.WriteLine(@"  或 :  ""C:\Windows\Notepad.exe""");
                    Console.WriteLine("         使用 RemoteHooking.CreateAndInject 创建新的notepad.exe进程");
                    Console.WriteLine();
                    Console.WriteLine("输入进程 Id 或可执行文件路径：");
                    Console.Write("> ");

                    args = new string[] { Console.ReadLine() };

                    Console.WriteLine();

                    if (String.IsNullOrEmpty(args[0])) return;
                }
                else
                {
                    targetExe = args[0];
                    break;
                }
            }
        }



    }
}
