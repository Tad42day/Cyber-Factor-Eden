using UnityEngine;
using System.Collections;

public class FootStepSounds : MonoBehaviour {

    public TextureType[] textureTypes;
    public AudioSource audioS;

    private AudioController ac;

    void Awake()
    {
        GameObject checkSoundTag = GameObject.FindGameObjectWithTag("AudioController");

        if (checkSoundTag != null)
        {
            ac = checkSoundTag.GetComponent<AudioController>();
        }
    }

    void PlayFootstepSound()
    {
        RaycastHit hit;
        Vector3 start = transform.position + transform.up;
        Vector3 dir = Vector3.down;

        if(Physics.Raycast(start, dir, out hit, 1.3f))
        {
            if (hit.collider.GetComponent<MeshRenderer>())
            {
                PlayMeshSound(hit.collider.GetComponent<MeshRenderer>());
            }
        }
    }

    void PlayMeshSound(MeshRenderer renderer)
    {
        if(audioS == null)
        {
            Debug.LogError("PlayMeshSound -- Não existe audioSource");
            return;
        }

        if(ac == null)
        {
            Debug.LogError("PlayMeshSound -- Sem audiocontroller");
            return;
        }

        if(textureTypes.Length > 0)
        {
            foreach (TextureType type in textureTypes)
            {
                if(type.footstepsSounds.Length == 0)
                {
                    return;
                }

                foreach (Texture tex in type.textures)
                {
                    if(renderer.material.mainTexture == tex)
                    {
                        ac.PlaySound(
                            audioS,
                            type.footstepsSounds[Random.Range(0, type.footstepsSounds.Length)],
                            true,
                            1f,
                            1.2f);
                    }
                }
            }
        }
    }
}

[System.Serializable]
public class TextureType
{
    public string name;
    public Texture[] textures;
    public AudioClip[] footstepsSounds;
}
