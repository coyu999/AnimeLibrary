#!HOOK MAIN
#!BIND HOOKED
#!SAVE BLURRED
#!DESC Blur background
vec4 hook() {
    vec2 uv = HOOKED_pos;
    vec4 color = vec4(0.0);
    float sigma = 10.0; // Blur intensity
    int radius = 5; // Blur radius
    float total = 0.0;
    for (int x = -radius; x <= radius; x++) {
        for (int y = -radius; y <= radius; y++) {
            float weight = exp(-(x*x + y*y) / (2.0 * sigma * sigma));
            color += weight * texture(HOOKED_tex, uv + vec2(x, y) / HOOKED_size);
            total += weight;
        }
    }
    return color / total;
}

#!HOOK MAIN
#!BIND HOOKED
#!BIND BLURRED
#!DESC Overlay original video
vec4 hook() {
    vec2 uv = HOOKED_pos;
    vec2 scaled_size = HOOKED_size * vec2(16.0/9.0, 1.0); // Scale to 16:9
    vec2 pos = uv * scaled_size / HOOKED_size;
    if (pos.x >= 0.0 && pos.x <= 1.0 && pos.y >= 0.0 && pos.y <= 1.0) {
        return texture(HOOKED_tex, pos); // Original video
    }
    return texture(BLURRED_tex, uv); // Blurred background
}