using UnityEngine;

public class FishCustomizer : MonoBehaviour
{
    private IKChain _chain;
    private FishProfile _baseProfile;
    public enum Species { Clownfish, Shark, Eel, Minnow }


    [Header("Species")]
    public Species species;

    [Header("Shape")]
    [Range(0.1f, 3f)] public float bodyWidth = 1f;
    [Range(0.1f, 3f)] public float bodyLength = 1f;
    [Range(0f, 1f)]   public float tailTaper = 0.5f;

    [Header("Behaviour")]
    [Range(0f, 1f)]   public float followSpeed = 0.12f;
    [Range(0f, 1f)]   public float stiffness = 0.5f;

    void Awake()
    {
        _chain = GetComponent<IKChain>();

        if (_chain.profile != null)
        {
            // A profile was assigned directly in the Inspector — use it as-is
            _baseProfile = _chain.profile;
            _chain.Init();
        }
        else
        {
            LoadSpecies();
        }
    }

    void LoadSpecies()
    {
        _baseProfile = species switch
        {
            Species.Clownfish => FishSpeciesLibrary.Clownfish(),
            Species.Shark     => FishSpeciesLibrary.Shark(),
            Species.Eel       => FishSpeciesLibrary.Eel(),
            Species.Minnow    => FishSpeciesLibrary.Minnow(),
            _                 => FishSpeciesLibrary.Clownfish()
        };

        _chain.profile = Object.Instantiate(_baseProfile);
        Rebuild();
    }

    void Rebuild()
    {
        if (_baseProfile == null || _chain?.profile == null) return;
        if (_chain.profile.segments.Length != _baseProfile.segments.Length) return;

        var segs = _chain.profile.segments;
        int count = segs.Length;

        for (int i = 0; i < count; i++)
        {
            float t = i / (float)(count - 1);
            segs[i].height       = _baseProfile.segments[i].height  * bodyWidth * Mathf.Lerp(1f, 1f - t, tailTaper);
            segs[i].length      = _baseProfile.segments[i].length * bodyLength;
            segs[i].lateralBend = _baseProfile.segments[i].lateralBend * Mathf.Lerp(1.5f, 0.5f, stiffness);
        }

        _chain.profile.followSpeed    = followSpeed;
        _chain.profile.solverIterations = _baseProfile.solverIterations;
        _chain.Init();
    }

    void OnValidate()
    {
        if (!Application.isPlaying) return;
        _chain = GetComponent<IKChain>();

        if (_chain.profile != null && _baseProfile == null)
        {
            // Profile was set externally, don't replace it
            _baseProfile = _chain.profile;
            Rebuild();
        }
        else
        {
            LoadSpecies();
        }
    }
}