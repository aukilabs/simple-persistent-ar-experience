using System.Collections;
using System.Collections.Generic;
using Auki.ConjureKit;
using Auki.ConjureKit.Manna;
using Auki.Integration.ARFoundation;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Random = UnityEngine.Random;

public class PersistentARinDomain : MonoBehaviour
{
    [SerializeField] private Camera arCamera;
    [SerializeField] private GameObject cube;
    [SerializeField] private GameObject calibrateUI;
    [SerializeField] private ARRaycastManager raycastManager;
    [SerializeField] private Button createCubeButton;

    private const string AppKey = "YOUR_APP_KEY";
    private const string AppSecret = "YOUR_APP_SECRET";

    private IConjureKit _conjureKit;
    private Manna _manna;
    private bool _calibrated = false;
    private List<ARRaycastHit> _arRaycastHits = new List<ARRaycastHit>();
    private SaveData _saveData = new SaveData();
    
    private void Start()
    {
        _conjureKit = new ConjureKit(
            arCamera.transform,
            AppKey,
            AppSecret);
        _manna = new Manna(_conjureKit);
    
        var textureProviderComp = CameraFrameProvider.GetOrCreateComponent();
        textureProviderComp.OnNewFrameReady += frame => _manna.ProcessVideoFrameTexture(frame.Texture, frame.ARProjectionMatrix, frame.ARWorldToCameraMatrix);
        _manna.OnLighthouseTracked += OnLighthouseTracked;
        
        createCubeButton.onClick.AddListener(OnCubeButtonClick);
    
        _conjureKit.Connect();
    }
    
    private void OnLighthouseTracked(Lighthouse lighthouse, Pose qrPose, bool isCalibrationGood)
    {
        // If the QR detection was good enough and the QR code is static (generated from the posemesh console),
        // hide the calibration view and show the cube marker 
        if (isCalibrationGood && lighthouse.Type == Lighthouse.LighthouseType.Static)
        {
            if(!_calibrated)
            {
                _calibrated = true;
                calibrateUI.SetActive(false);
                cube.SetActive(true);
                LoadLocally();
            }
        }
    }

    private void Update()
    {
        // Make a raycast from the center of the screen to an AR plane (floor, wall, or any other surface detected by ARFoundation)
        var ray = arCamera.ViewportPointToRay(Vector3.one * 0.5f);
        if (raycastManager.Raycast(ray, _arRaycastHits, TrackableType.PlaneWithinPolygon))
        {
            // Place the cube where the raycast hits a plane. Move it half the cube size along the hit normal (up if on the ground, forward if on the wall)
            cube.transform.position = _arRaycastHits[0].pose.position + _arRaycastHits[0].pose.up * cube.transform.localScale.x / 2f;
            // Rotate the cube only around y axis to always face the camera
            cube.transform.rotation = Quaternion.Euler(Vector3.Scale(arCamera.transform.rotation.eulerAngles, Vector3.up));
        }
    }
    
    private void PlaceCube(Vector3 position, Quaternion rotation, Color color)
    {
        var placedCube = Instantiate(cube, position, rotation);
        placedCube.GetComponent<Renderer>().material.color = color;
        placedCube.gameObject.SetActive(true);
    }
    
    private void OnCubeButtonClick()
    {
        var color = Random.ColorHSV();
        // Place the cube where the cube marker is 
        PlaceCube(cube.transform.position, cube.transform.rotation, color);
        // Save the position and rotation information locally
        _saveData.cubes.Add(new CubeData(cube.transform.position, cube.transform.rotation, color));
        SaveLocally();
    }
    
    private void SaveLocally()
    {
        var json = JsonUtility.ToJson(_saveData);
        PlayerPrefs.SetString("_saveData", json);
        PlayerPrefs.Save();
    }
    
    private void LoadLocally()
    {
        if(!PlayerPrefs.HasKey("_saveData"))
            return;
    
        var json = PlayerPrefs.GetString("_saveData");
        _saveData = JsonUtility.FromJson<SaveData>(json);

        foreach (var savedCube in _saveData.cubes)
        {
            PlaceCube(savedCube.position.ToVector3(), savedCube.rotation.ToQuaternion(), savedCube.color.ToColor());
        }
    }
}
