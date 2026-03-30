using UnityEngine;

public class WaypointMarker : MonoBehaviour
{

    public WaypointType type = WaypointType.Navigation;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}

public enum WaypointType { Navigation, Search }