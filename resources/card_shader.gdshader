shader_type spatial;
render_mode unshaded, depth_draw_always;

// --- Depth Control ---
uniform float depth_setting : hint_range(0.0, 1.0, 0.001) = 0.0;

// --- Main Texture ---
uniform sampler2D albedo_texture : source_color;
uniform vec4 modulate_color : source_color = vec4(1.0);
uniform float aspect_ratio = 1.5;

// --- Border Effect ---
uniform vec4 line_color : source_color = vec4(1.0);
uniform float line_thickness : hint_range(0, 0.1) = 0.01; // Changed range to work with UV coordinates
uniform float fade_amount : hint_range(0, 1) = 1.0;

// --- Border Reveal Effect ---
uniform float border_progress : hint_range(0, 1) = 1.0;

// --- Alpha Scissor Threshold ---
uniform float alpha_cutoff : hint_range(0, 1) = 0.2; // NEW: Added cutoff parameter

// Sample pattern for the outline
const vec2 OFFSETS[8] = vec2[8](
	vec2(-1, -1), vec2(-1, 0), vec2(-1, 1),
	vec2(0, -1),                vec2(0, 1),
	vec2(1, -1),  vec2(1, 0),   vec2(1, 1)
);

void fragment() {
	// 1. Sample the main texture
	vec4 color = texture(albedo_texture, UV);
	color *= modulate_color;

	// --- Aspect Ratio Correction ---
	vec2 aspect_corrected_thickness = vec2(line_thickness, line_thickness / aspect_ratio);
	// -----------------------------------------

	// 2. Apply the border outline effect
	// Use a fixed size based on line_thickness (now in UV space, 0-1 range)
	float outline_alpha = 0.0;

	for (int i = 0; i < OFFSETS.length(); i++) {
		// Sample around the current UV position
		vec4 sample_color = texture(albedo_texture, UV + OFFSETS[i] * aspect_corrected_thickness);
		outline_alpha += sample_color.a;
	}
	outline_alpha = min(outline_alpha, 1.0);

	vec4 outline_color = line_color;
	outline_color.a *= fade_amount;

	// Mix between the main color and the outline
	vec4 final_color = mix(color, outline_color, outline_alpha - color.a);

	// 3. Apply the progress-based border reveal effect
	vec2 centered_uv = UV * 2.0 - 1.0; // Convert UV to range -1 to 1
	float distance_from_center = max(abs(centered_uv.x), abs(centered_uv.y));

	// Use alpha scissoring for the reveal effect
	if (distance_from_center > border_progress) {
		discard; // This fragment is not rendered
	}

    // 4. Alpha scissor test - discard fragments below cutoff
	// Use the combined alpha from your final color calculation
	if (final_color.a < alpha_cutoff) {
		discard;
	}

	// 4. Set the final output for visible fragments
	ALBEDO = final_color.rgb;
	ALPHA = final_color.a;
	DEPTH = depth_setting;
}