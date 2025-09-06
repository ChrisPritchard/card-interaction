shader_type spatial;

render_mode unshaded;

// Texture uniform - assign your texture here in the material
uniform sampler2D albedo_texture : filter_nearest;
uniform vec4 albedo_color : source_color = vec4(1.0);

uniform vec4 border_color : source_color = vec4(1.0);
uniform vec4 border_mask : source_color = vec4(1.0, 0.0, 0.8627, 1.0);
uniform float color_tolerance : hint_range(0.0, 0.1, 0.001) = 0.01;

void fragment() {
    // Sample the texture normally
    vec4 tex_color = texture(albedo_texture, UV);
    
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
}