namespace Snow.Terrain;

public sealed partial class SnowTerrain
{
	private const string CLEAR_DEFORMATION_COMPUTE = "shaders/computes/cleardeformation.shader";
	private const string UPDATE_DEFORMATION_COMPUTE = "shaders/computes/updatedeformation.shader";
	private const string UPDATE_MASK_COMPUTE = "shaders/computes/updatesnowmask.shader";
}
