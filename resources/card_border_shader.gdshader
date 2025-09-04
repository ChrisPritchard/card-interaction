shader_type canvas_item;

uniform vec4 border_color : source_color = vec4(1.0, 1.0, 1.0, 1.0);
uniform float border_width : hint_range(0, 0.2) = 0.1;
uniform float fade_amount : hint_range(0, 1) = 0.0;

void fragment() {
    vec4 tex_color = texture(TEXTURE, UV);
    float alpha = tex_color.a;
    
    // Calculate distance from edge
    vec2 uv_offset = abs(UV - 0.5) * 2.0;
    float dist = max(uv_offset.x, uv_offset.y);
    
    // Create border effect
    if (dist > 1.0 - border_width && alpha > 0.0) {
        COLOR = mix(tex_color, border_color, fade_amount);
    } else {
        COLOR = tex_color;
    }
}