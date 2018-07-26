using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Deployment.WindowsInstaller;

namespace CustomAction
{
    public class CustomActions
    {
        [CustomAction]
        public static ActionResult CustomAction1(Session session)
        {
            session.Log("Begin CustomAction");

            session.Message(InstallMessage.Warning, new Record
            {
                FormatString = "CustomAction was called after SetupProject1 installation was finished."
            });

            return ActionResult.Success;
        }
    }
}
