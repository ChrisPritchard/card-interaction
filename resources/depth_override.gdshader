shader_type spatial;

render_mode depth_draw_always;

// Texture uniform - assign your texture here in the material
uniform sampler2D albedo_texture : source_color;
uniform vec4 albedo_color : source_color = vec4(1.0);
uniform float depth_setting : hint_range(0.0, 1.0) = 0.0;

void fragment() {
    // Sample the texture normally
    vec4 tex_color = texture(albedo_texture, UV);
    
    // Apply the texture with color tint
    ALBEDO = tex_color.rgb * albedo_color.rgb;
    ALPHA = tex_color.a * albedo_color.a;
    
    // Override depth to always be at the near plane (always in front)
    DEPTH = depth_setting;
}