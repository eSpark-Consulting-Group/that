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
        Confirmation
    }

    public sealed class ChangeScreen
    {
        public ChangeScreenTypes Screen { get; private set; }
        public Action<ChangeScreenTypes> Action { get; private set; }
        public Direction? Direction { get; private set; }

        public ChangeScreen(ChangeScreenTypes screen, Action<ChangeScreenTypes> action, Direction? direction = null)
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
            public static void Send(ChangeScreenTypes message, Action<ChangeScreenTypes> action, Direction? direction = null)
            {
                MessageManager.Instance.PublishMessage(new ChangeScreen(message, action, direction));
            }
        }
    }
}