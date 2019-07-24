using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YuanshengHook
{
    public class ServerInterface : MarshalByRefObject
    {
        public void IsInstalled(int clientPID)
        {
            Console.WriteLine("代码已经被注入到进程 {0} .\r\n", clientPID);
        }

        public void ReportMessages(string[] messages)
        {
            for(int i=0; i<messages.Length; i++)
            {
                Console.WriteLine(messages[i]);
            }
        }

        public void ReportMessage(string message)
        {
            Console.WriteLine(message);
        }

        public void ReportException(Exception e)
        {
            Console.WriteLine("目标进程抛出异常：\r\n" + e.ToString());
        }

        int count = 0;
        public void Ping()
        {
            //动画显示可视化的Ping
            var oldTop = Console.CursorTop;
            var oldLeft = Console.CursorLeft;
            Console.CursorVisible = false;

            var chars = "\\|/-";
            Console.SetCursorPosition(Console.WindowWidth - 1, oldTop - 1);
            Console.Write(chars[count++ % chars.Length]);

            Console.SetCursorPosition(oldLeft, oldTop);
            Console.CursorVisible = true;
        }

        public void Disconnect()
        {
            SafeInvokeDisconnected();
        }



        private void SafeInvokeDisconnected()
        {
            Console.WriteLine("disconnected...");

        }
    }
}
