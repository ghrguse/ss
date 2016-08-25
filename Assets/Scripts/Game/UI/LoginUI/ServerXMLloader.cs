using UnityEngine;
using System.Collections;
using Utils.Event;

public class ServerXMLloader : MonoBehaviour 
{
	private bool isDownloading = false;
	private WWW www = null;

	private static ServerXMLloader loader;
	private static ServerSelectModel model;

	public static void startLoad ( ServerSelectModel model_ )
	{
		model = model_;
		if ( loader != null )
			Destroy ( loader.gameObject );

		loader = new GameObject ( "serverXMLloader" ).AddComponent<ServerXMLloader> ();
	}

	void Start ()
	{
        StartCoroutine(LoadHttpXML());
	}


	void Update()
	{
		
	}

    private IEnumerator LoadHttpXML()
    {
        isDownloading = true;
        www = new WWW(model.dirServerURL);
        yield return www;

        isDownloading = false;
        if (!string.IsNullOrEmpty(www.error))
        {
            Log.info("LoadHttpXML failed:" + model.dirServerURL + "," + www.error);
        }
        else
        {
            model.parseSvrListXML(www.text);
            Destroy(gameObject);
        }
    }

	void OnDestroy ()
	{

	}

}
