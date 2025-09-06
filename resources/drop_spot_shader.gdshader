shader_type spatial;

render_mode unshaded; // necessary to stop the colour drifting from the mask colour too much

uniform sampler2D albedo_texture : filter_nearest; // nearest blending also assists with ^, and disabling mipmaps on the import
uniform vec4 albedo_color : source_color = vec4(1.0);

uniform vec4 border_color : source_color = vec4(1.0);
uniform vec4 border_mask : source_color = vec4(1.0, 0.0, 0.8627, 1.0);
uniform float color_tolerance : hint_range(0.0, 0.1, 0.001) = 0.01;

void fragment() {
    vec4 tex_color = texture(albedo_texture, UV);

    float color_diff = distance(tex_color.rgb, border_mask.rgb);
    if (color_diff < color_tolerance) {
        ALBEDO = border_color.rgb;
        ALPHA = border_color.a;
    } else {
        ALBEDO = tex_color.rgb * albedo_color.rgb;
        ALPHA = tex_color.a * albedo_color.a;
    }
}