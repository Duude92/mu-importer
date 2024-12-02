using UnityEngine;

namespace MUImporter
{
    public class BMD : MonoBehaviour
    {
        public Vector3 startPos = Vector3.zero;
        public Vector3 startRot = Vector3.zero;
        [SerializeField] BMDObject _bmdObject;
        public BMDObject.BMD_MESH[] Meshes { get => _bmdObject.meshes; }
        public BMDObject.BMD_ANIM[] Anims { get => _bmdObject.anims; }
        public BMDObject.BMD_BONE[] Bones { get => _bmdObject.bones; }

        public void FromBytes(byte[] buffer)
        {
            _bmdObject.FromBytesM(buffer);
        }
    }
}