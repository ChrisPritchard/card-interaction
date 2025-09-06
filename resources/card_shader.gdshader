shader_type spatial;
render_mode depth_test_disabled, shadows_disabled;

// --- Main Texture ---
uniform sampler2D albedo_texture : source_color;
uniform vec4 modulate_color : source_color = vec4(1.0);
uniform float aspect_ratio = 1.5;

// --- Border Effect ---
uniform vec4 line_color : source_color = vec4(1.0);
uniform float line_thickness : hint_range(0, 0.1) = 0.01;
uniform float fade_amount : hint_range(0, 1) = 1.0;
uniform float border_progress : hint_range(0, 1) = 1.0;

const vec2 OFFSETS[8] = vec2[8](
	vec2(-1, -1), vec2(-1, 0), vec2(-1, 1),
	vec2(0, -1),                vec2(0, 1),
	vec2(1, -1),  vec2(1, 0),   vec2(1, 1)
);

void fragment() {
	vec4 color = texture(albedo_texture, UV);
	color *= modulate_color;

    // the uv offsets are 0.0 to 1.0 in each direction, regardless of texture dimensions. so cards for example are slightly compressed in the vertical axis
    // this code corrects for that, by applying the x to y aspect ratio
	vec2 aspect_corrected_thickness = vec2(line_thickness, line_thickness / aspect_ratio);
	float outline_alpha = 0.0;
	for (int i = 0; i < OFFSETS.length(); i++) {
		vec4 sample_color = texture(albedo_texture, UV + OFFSETS[i] * aspect_corrected_thickness);
		outline_alpha += sample_color.a;
	}

    // the sum of the sampled textures will be 0 when they're all transparent,
    // 1 when all opaque, and some mix when there is an edge present
	outline_alpha = min(outline_alpha, 1.0);

	vec4 outline_color = line_color;
	outline_color.a *= fade_amount;

	vec4 final_color = mix(color, outline_color, outline_alpha - color.a);

	vec2 centered_uv = UV * 2.0 - 1.0;
	float distance_from_center = max(abs(centered_uv.x), abs(centered_uv.y));

	if (distance_from_center > border_progress) {
		discard; // This fragment is not rendered
	}

	ALBEDO = final_color.rgb;
	ALPHA = final_color.a;
}