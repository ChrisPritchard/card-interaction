shader_type spatial;

render_mode unshaded, depth_draw_always;

// Texture uniform - assign your texture here in the material
uniform sampler2D albedo_texture : filter_nearest;
uniform vec4 albedo_color : source_color = vec4(1.0);

uniform vec4 border_color : source_color = vec4(1.0);
uniform vec4 border_mask : source_color = vec4(1.0, 0.0, 0.8627, 1.0);
uniform float color_tolerance : hint_range(0.0, 0.1, 0.001) = 0.01;

uniform float depth_setting : hint_range(0.0, 1.0, 0.001) = 0.0;

// --- Alpha Scissor Threshold ---
uniform float alpha_cutoff : hint_range(0, 1) = 0.2; // NEW: Added cutoff parameter

void fragment() {
    // Sample the texture normally
    vec4 tex_color = texture(albedo_texture, UV);
	
	// Use the combined alpha from your final color calculation
	if (tex_color.a < alpha_cutoff) {
		discard;
	}
    
    // Check if the current pixel color matches the border mask color
    // Using a small tolerance to account for texture compression/artifacts
    float color_diff = distance(tex_color.rgb, border_mask.rgb);
    
    if (color_diff < color_tolerance) {
        // Replace with border color
        ALBEDO = border_color.rgb;
        ALPHA = border_color.a;
    } else {
        // Apply the texture with color tint normally
        ALBEDO = tex_color.rgb * albedo_color.rgb;
        ALPHA = tex_color.a * albedo_color.a;
    }
    
    // Override depth to always be at the near plane (always in front)
    DEPTH = depth_setting;
}