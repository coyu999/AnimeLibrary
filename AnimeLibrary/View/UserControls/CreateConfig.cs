using System.IO;
using System.Text.Json;

namespace AnimeLibrary.View.UserControls
{
    public class CreateConfig
    {
        public static void CreateConfigFile() 
        {
            var config = new 
            { 
                directory = "", 
                currentUpscale = "", 
                discordRPC = false, 
                englishTitle = false, 
                hideOnRun = false,
                A = new List<string>
                {
                    "Anime4K_Clamp_Highlights.glsl",
                    "Anime4K_Restore_CNN_VL.glsl",
                    "Anime4K_Upscale_CNN_x2_VL.glsl",
                    "Anime4K_AutoDownscalePre_x2.glsl",
                    "Anime4K_AutoDownscalePre_x4.glsl",
                    "Anime4K_Upscale_CNN_x2_M.glsl"
                },
                B = new List<string> {
                    "Anime4K_Clamp_Highlights.glsl",
                    "Anime4K_Restore_CNN_Soft_VL.glsl",
                    "Anime4K_Upscale_CNN_x2_VL.glsl",
                    "Anime4K_AutoDownscalePre_x2.glsl",
                    "Anime4K_AutoDownscalePre_x4.glsl",
                    "Anime4K_Upscale_CNN_x2_M.glsl"
                },
                C = new List<string> {
                    "Anime4K_Clamp_Highlights.glsl",
                    "Anime4K_Upscale_Denoise_CNN_x2_VL.glsl",
                    "Anime4K_AutoDownscalePre_x2.glsl",
                    "Anime4K_AutoDownscalePre_x4.glsl",
                    "Anime4K_Upscale_CNN_x2_M.glsl"
                },
                AA = new List<string>
                {
                    "Anime4K_Clamp_Highlights.glsl",
                    "Anime4K_Restore_CNN_VL.glsl",
                    "Anime4K_Upscale_CNN_x2_VL.glsl",
                    "Anime4K_Restore_CNN_M.glsl",
                    "Anime4K_AutoDownscalePre_x2.glsl",
                    "$Anime4K_AutoDownscalePre_x4.glsl",
                    "Anime4K_Upscale_CNN_x2_M.glsl"
                },
                BB = new List<string>
                {
                    "Anime4K_Clamp_Highlights.glsl",
                    "Anime4K_Restore_CNN_Soft_VL.glsl",
                    "Anime4K_Upscale_CNN_x2_VL.glsl",
                    "Anime4K_AutoDownscalePre_x2.glsl",
                    "Anime4K_AutoDownscalePre_x4.glsl",
                    "Anime4K_Restore_CNN_Soft_M.glsl",
                    "Anime4K_Upscale_CNN_x2_M.glsl"
                },
                CA = new List<string>
                {
                    "Anime4K_Clamp_Highlights.glsl",
                    "Anime4K_Upscale_Denoise_CNN_x2_VL.glsl",
                    "Anime4K_AutoDownscalePre_x2.glsl",
                    "Anime4K_AutoDownscalePre_x4.glsl",
                    "Anime4K_Restore_CNN_M.glsl",
                    "Anime4K_Upscale_CNN_x2_M.glsl"
                }
            };
            var options = new JsonSerializerOptions { WriteIndented = true };
            string configJson = JsonSerializer.Serialize(config, options);

            File.WriteAllText("config.json", configJson);
        }
    }
}
