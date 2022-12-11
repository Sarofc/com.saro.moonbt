using System.Text;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Saro.BT
{
    [MovedFrom(true, sourceClassName: "Print")]
    [BTNode("Task_24x", "“打印日志”")]
    public sealed class PrintLog : BTTask
    {
        public enum ELogType : byte { Info, Warning, Error }

        [TextArea]
        public string message = "Print Node";

        [Tooltip("the type of message to display.")]
        public ELogType logType = ELogType.Info;

        public override EStatus OnExecute(float deltaTime)
        {
            switch (logType)
            {
                case ELogType.Info:
                    Log.INFO(message);
                    break;
                case ELogType.Warning:
                    Log.WARN(message);
                    break;
                case ELogType.Error:
                    Log.ERROR(message);
                    break;
                default:
                    break;
            }

            return EStatus.Success;
        }

        public override void Description(StringBuilder builder)
        {
            // Nothing to display.
            if (message.Length == 0)
            {
                return;
            }

            string displayed = message;

            // Only consider display the message up to the newline.
            int newLineIndex = message.IndexOf('\n');
            if (newLineIndex >= 0)
            {
                displayed = message.Substring(0, newLineIndex);
            }

            // Nothing to display.
            if (displayed.Length == 0)
                return;

            if (logType != ELogType.Info)
            {
                builder.Append("LogType: ");
                builder.AppendLine(logType.ToString());
            }

            // Cap the message length to display to keep things compact.
            int maxCharacters = 20;
            if (displayed.Length > maxCharacters)
            {
                builder.Append(displayed.Substring(0, maxCharacters));
                builder.Append("...");
            }
            else
            {
                builder.Append(displayed);
            }
        }
    }
}