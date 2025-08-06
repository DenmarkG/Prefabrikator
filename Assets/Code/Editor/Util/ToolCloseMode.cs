
namespace Prefabrikator // #DG: TODO move to editor namespace
{
    [System.Flags]
    public enum ToolCloseMode : int
    {
        None = 0,
        Save = 0x1,
        Continue = 0x2,
        Cancel = 0x4,
        TearDownModifiers = 0x8,
    }
}