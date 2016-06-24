using System;

namespace CodedUITestAT
{
    /// <summary> Contains helper methods related to work with strings and texts. </summary>
    internal static class TextHelper
    {
        /// <summary> Generates a new unique name. </summary>
        internal static string GenerateName()
        {
            return @"TestName" + DateTime.Now.ToString("yyyyMMddHHmmss");
        }
    }
}
