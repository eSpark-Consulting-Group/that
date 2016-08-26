using DDT.Application.Transitions;
using System;

namespace DDT.Application.Messages
{
    public enum ChangeScreenTypes
    {
        EULA,
        Packages,
        DiscoveryTargets,
        ActiveDirectoryCredentials,
        WorkgroupCredentials,
        ActiveDirectory,
        Workgroup,
        NetworkAddresses,
        DeploymentCredentials,
        DeploymentResults,
        Report,
        Options,
        About,
        StopConfirmation, 
        SaveConfirmation
    }

    public sealed class ChangeScreen<T>
    {
        public ChangeScreenTypes Screen { get; private set; }
        public Action<T> Action { get; private set; }
        public Direction? Direction { get; private set; }

        public ChangeScreen(ChangeScreenTypes screen, Action<T> action, Direction? direction = null)
        {
            Screen = screen;
            Action = action;
            Direction = direction;
        }
    }

    public static class AppMessages
    {
        public static class ChangeScreenMessage
        {
            public static void Send<T>(ChangeScreenTypes message, Action<T> action, Direction? direction = null)
            {
                MessageManager.Instance.PublishMessage(new ChangeScreen<T>(message, action, direction));
            }
        }
    }
}