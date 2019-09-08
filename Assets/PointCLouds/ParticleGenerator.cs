using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleGenerator : MonoBehaviour
{
    // -------------------------------------------------------------------------------------------------------------------------------------------
    [System.Serializable]
    public struct MeshSettings
    {
        public GameObject PointCacheSource;
        public Texture meshTexture;
        public float particleSize;
        public float meshScaleFactor;
        public Vector3 meshPosOffset;

        public ComputeBuffer MeshDataBuffer;
        public ComputeBuffer MeshUVDataBuffer;

        [HideInInspector]
        public Mesh meshToBake;
    }

    struct ParticleData
    {
        public Vector4 Position;
        public Vector4 Color;
    }

    struct MeshData
    {
        public Vector3 Position;
    }

    // -------------------------------------------------------------------------------------------------------------------------------------------
    public ComputeShader cp;
    public int numberOfParticles = 60000;
    public Material mt;

    [Range(0,1)]
    public float ColorizationStrength;
    [Range(0,1)]
    public float MeshOneTwoTransition;
    public bool autoTransition;


    [Header("Mesh")]
    public MeshSettings MeshOne;
    public MeshSettings MeshTwo;

    [Header("Coloring")]
    public Color KeyOne =new Color(0.4588236f,0.4901961f,0.3647059f);
    public Color KeyTwo = new Color(0.8509805f, 0.2901961f, 0.007843138f);
    public Color KeyThree = new Color(0.9245283f, 0.7576204f, 0.4099323f);
    public Color FogColor = new Color(0.9188747f, 1, 0.3113208f);

    ComputeBuffer ParticleBuffer;
    ComputeBuffer OldParticleBuffer;
    
    bool isOld;
    // -------------------------------------------------------------------------------------------------------------------------------------------

    void Start()
    {
        PopulateBufferWithMeshPositions(ref MeshOne);
        PopulateBufferWithMeshPositions(ref MeshTwo);
        ParticleBuffer = new ComputeBuffer(numberOfParticles, sizeof(float) * 8);
        OldParticleBuffer = new ComputeBuffer(numberOfParticles, sizeof(float) * 8);
 
    }

    // Update is called once per frame
    void Update()
    {
        if (autoTransition)
        {
            MeshOneTwoTransition = Mathf.Clamp01( Mathf.Abs( Mathf.Sin(Time.time) )*2f -0.5f);
        }
        ExecuteComputeShader();
    }


    private void OnRenderObject()
    {
        Rendershapes();
    }



    private void OnDestroy()
    {
        ParticleBuffer.Release();
        OldParticleBuffer.Release();
    }

    // -------------------------------------------------------------------------------------------------------------------------------------------
    void Rendershapes()
    {
        mt.SetPass(0);
        Matrix4x4 ow = this.transform.localToWorldMatrix;
        mt.SetMatrix("My_Object2World", ow);

        if (isOld) mt.SetBuffer("_ParticleDataBuff", OldParticleBuffer);
        else mt.SetBuffer("_ParticleDataBuff", ParticleBuffer);
        mt.SetColor("_FogColorc", FogColor);
        mt.SetFloat("_ParticleSize", Mathf.Lerp(MeshOne.particleSize, MeshTwo.particleSize, MeshOneTwoTransition));
        Graphics.DrawProceduralNow(MeshTopology.Triangles, 3 * 4, numberOfParticles);
    }
    
    void ExecuteComputeShader()
    {
        int kernelHandle = cp.FindKernel("CSMain");
        if (isOld)
        {
            cp.SetBuffer(kernelHandle ,"_inParticleBuffer", ParticleBuffer);
            cp.SetBuffer(kernelHandle, "_outParticleBuffer", OldParticleBuffer);
        }else
        {
            cp.SetBuffer(kernelHandle, "_inParticleBuffer", OldParticleBuffer);
            cp.SetBuffer(kernelHandle, "_outParticleBuffer", ParticleBuffer);
        }

        //Setting meshOne variables
        cp.SetBuffer(kernelHandle, "_MeshDataOne", MeshOne.MeshDataBuffer);
        cp.SetBuffer(kernelHandle, "_MeshDataUVOne", MeshOne.MeshUVDataBuffer);
        cp.SetInt("_CachePointVertexcoundOne", MeshOne.meshToBake.vertexCount);
        cp.SetTexture(kernelHandle, "_MeshTextureOne", MeshOne.meshTexture);
        cp.SetVector("_transformInfoOne", new Vector4(MeshOne.meshPosOffset.x, MeshOne.meshPosOffset.y,
            MeshOne.meshPosOffset.z, MeshOne.meshScaleFactor));

        //Setting meshTwo variables
        cp.SetBuffer(kernelHandle, "_MeshDataTwo", MeshTwo.MeshDataBuffer);
        cp.SetBuffer(kernelHandle, "_MeshDataUVTwo", MeshTwo.MeshUVDataBuffer);
        cp.SetInt("_CachePointVertexcoundTwo", MeshTwo.meshToBake.vertexCount);
        cp.SetTexture(kernelHandle, "_MeshTextureTwo", MeshTwo.meshTexture);
        cp.SetVector("_transformInfoTwo", new Vector4(MeshTwo.meshPosOffset.x, MeshTwo.meshPosOffset.y,
       MeshTwo.meshPosOffset.z, MeshTwo.meshScaleFactor));

        cp.SetFloat("meshOneTwoTransition", MeshOneTwoTransition);
        cp.SetInt("_NumberOfParticles", numberOfParticles);
        cp.SetFloat("_Time", Time.time);
        KeyOne.a = ColorizationStrength;
        cp.SetVector("_Color1", KeyOne);
        cp.SetVector("_Color2", KeyTwo);
        cp.SetVector("_Color3", KeyThree);
        cp.SetVector("CameraPosition", this.transform.InverseTransformPoint(Camera.main.transform.position));
        cp.SetVector("CameraForward", this.transform.InverseTransformDirection(Camera.main.transform.forward));

        cp.Dispatch(kernelHandle, numberOfParticles, 1, 1);
        isOld = !isOld;


    }



    void PopulateBufferWithMeshPositions(ref MeshSettings toSet)
    {
        MeshFilter m = toSet.PointCacheSource.GetComponent<MeshFilter>();
        if (m == null) m = toSet.PointCacheSource.GetComponentInChildren<MeshFilter>();
        toSet.meshToBake = m.sharedMesh;


       
        int stride =numberOfParticles / toSet.meshToBake.vertexCount;
        toSet.MeshDataBuffer = new ComputeBuffer(toSet.meshToBake.vertexCount, sizeof(float) * 3);


        toSet.MeshDataBuffer.SetData(toSet.meshToBake.vertices);


        toSet.MeshUVDataBuffer = new ComputeBuffer(toSet.meshToBake.vertexCount, sizeof(float) * 2);

        toSet.MeshUVDataBuffer.SetData(toSet.meshToBake.uv);
    }


   
}
