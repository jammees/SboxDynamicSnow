using Sandbox;

namespace Snow.Utility;

internal static class Constants
{
	[SkipHotload]
	public const string CLEAR_DEFORMATION_COMPUTE = "shaders/computes/cleardeformation.shader";

	[SkipHotload]
	public const string UPDATE_DEFORMATION_COMPUTE = "shaders/computes/updatedeformation.shader";

	[SkipHotload]
	public const string UPDATE_MASK_COMPUTE = "shaders/computes/updatesnowmask.shader";

	[SkipHotload]
	public const string CREATE_CONTROL_MAP_COMPUTE = "shaders/computes/control/createcontrolmap.shader";
}
