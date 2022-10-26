#version 330 core
in vec2 fUv;

//A uniform of the type sampler2D will have the storage value of our texture.
uniform sampler2D uTexture0;
uniform float time;


out vec4 FragColor;

void main()
{
    //Here we sample the texture based on the Uv coordinates of the fragment
    vec4 textureColor = texture(uTexture0, fUv);
    if(textureColor.a < 0.1f) 
        FragColor = vec4(1, 1, 1,1) * vec4(sin(time), cos(time), 1, 1);
    else FragColor = textureColor;
}