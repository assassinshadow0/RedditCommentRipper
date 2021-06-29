using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Linq;
using UnityEngine.UI;
using UnityEditor;
using UnityEngine.SceneManagement;


public class MainWorker : MonoBehaviour
{
	public InputField url_input, maxPosts_input, messages_input, console_input, minCharCount_input, maxCharCount_input;
    public Button submit;

	int totalPosts =25;
	private int minCharCount = 3;
	private int maxCharCount = 300;
	string after =null;
	int errorDl = 0;
	public Image progressImageMain;
	public Image progressImageDL;

	int downloaded = 0;
	List<string> postLinks = new List<string>();

	public GameObject shade;

	public TagItemObject tagPrefab;

	public GameObject acceptPanel;
	public GameObject denyPanel;

	private bool isProcessDone = false;
	private bool hasStartedProcess = false;
	

	public List<string> wordFilterAccept;
	public List<string> wordFilterDeny;
	public List<CommentsClass> comments;

    void Start()
    {
		
	    
	    Debug.Log("Save Location: " + Application.persistentDataPath + "/comments.txt");
		System.Net.ServicePointManager.ServerCertificateValidationCallback
		= (a, b, c, d) => { return true; };
		
		//UnityTask.InitialiseDispatcher ();
		console_input.text = Application.persistentDataPath + "/comments.txt";
		
		submit.onClick.AddListener (Submit);
		comments = new List<CommentsClass>();

	
		foreach (Transform child in acceptPanel.transform) 
		{
			if (child.gameObject.GetComponent<TagItemObject>() != null)
			{
				Destroy(child.gameObject);
			}
		}
		
		foreach (Transform child in denyPanel.transform) 
		{
			if (child.gameObject.GetComponent<TagItemObject>() != null)
			{
				Destroy(child.gameObject);
			}
		}
		
		
		for (int i =0; i < wordFilterAccept.Count; i++)
		{
			// CommentsClass newClass = new CommentsClass();
			// newClass.commentId = wordFilterAccept[i];
			// comments.Add(newClass);
			
			TagItemObject newtag  = Instantiate(tagPrefab, acceptPanel.transform, true);
			
			newtag.transform.localScale = new Vector3(1,1,1);

			newtag.tagName.text = wordFilterAccept[i];
			newtag.worker = this;
			newtag.isAcceptPanel = true;
			newtag.index = i;
		}	
		
		for (int i =0; i < wordFilterDeny.Count; i++)
		{

			TagItemObject newtag  = Instantiate(tagPrefab, denyPanel.transform, true);
			
			newtag.transform.localScale = new Vector3(1,1,1);

			newtag.tagName.text = wordFilterDeny[i];
			newtag.worker = this;
			newtag.isAcceptPanel = false;
			newtag.index = i;
		}
		



    }


    
    void Submit()
    {
	    if (!hasStartedProcess && !isProcessDone)
	    {
		    hasStartedProcess = true;
		    shade.SetActive(true);
		    
		    for (int i =0; i < wordFilterAccept.Count; i++)
		    {
			    CommentsClass newClass = new CommentsClass();
			    newClass.commentId = wordFilterAccept[i];
			    comments.Add(newClass);
		    }
		    
		    int.TryParse(maxPosts_input.text, out totalPosts);
		    int.TryParse(minCharCount_input.text, out minCharCount);
		    int.TryParse(maxCharCount_input.text, out maxCharCount);
		    
		    if (totalPosts == 0)
		    {
			    totalPosts = 25;
		    }

		    url_input.readOnly = true;
		    minCharCount_input.readOnly = true;
		    maxCharCount_input.readOnly = true;
		    maxPosts_input.readOnly = true;
		    
		    UpdateMessages("Starting Process");

		    if (postLinks.Count != 0)
		    {
			    postLinks.Clear();
		    }
		    UpdateMessages("Fetching...");
		    StartCoroutine(StartFetching());
	    }
	    else if (isProcessDone)
	    {
		    SceneManager.LoadScene("u");
	    }
    }

    
    #region UI_Related

    void UpdateMessages(string message)
    {
	    messages_input.text = message;
    }

    void UpdateConsoleMessages(string message)
    {
	    console_input.text = message;
    }
    

    #endregion

    
    
    #region Fetchers

    private IEnumerator StartFetching()
    {
	    UpdateMessages("Waiting... Saved - " + postLinks.Count + " / " + totalPosts);
	    string url = url_input.text;
	    if(!url.EndsWith(".json"))
	    {
		    url = url + ".json";
	    }

	    if (after != null && after.Length > 0) {
		    url = url + "?count=25&after=" + after;
	    }

	    yield return new WaitForSeconds(5);
		
	    UpdateConsoleMessages(url);

	    fetchMainPosts (url);

    }
    
	void fetchMainPosts(string url)
	{
		UpdateMessages("Fetching Posts");
		UnityHTTP.Request someRequest = new UnityHTTP.Request( "get", url );
		someRequest.Send( ( request ) => {
			
			UpdateMessages("Found Posts"); // tell people that posts were found

			JSONObject data = new JSONObject(request.response.Text); //get the json data

			JSONObject inData = data.GetField("data");
			
			after = inData.GetField("after").str;

			JSONObject kids = inData.GetField("children");

			//loop through the post and get the comment urls as well
			foreach(JSONObject j in kids.list) 
			{
				JSONObject dataInChild = j.GetField("data");
				if (dataInChild.HasField("permalink") )
				{
					string contentUrl = "https://www.reddit.com" + dataInChild.GetField("permalink").str + ".json";
					//add to list for later parsing
					postLinks.Add(contentUrl);
					
					

					string msgs = dataInChild.GetField("title").str;

					UpdateConsoleMessages(msgs);
					
					//display progress in text and progress form
					int count = postLinks.Count;
					progressImageMain.fillAmount = ((float) count / totalPosts);
					UpdateMessages(" Adding Posts " + count + " / " + totalPosts); 

					//just in case checks
					if (postLinks.Count >= totalPosts)
					{
						break;
					}
					


				}
					
			}
				
			//can loop due to having legal index, keep fetching urls
			if (postLinks.Count < totalPosts) 
			{
				StartCoroutine (StartFetching ());
			}
			else 
			{
				//got all the urls so now we are going to download the comments
				UpdateMessages(" Posts Loaded... Downloading comments now...." );
				fetchComment(postLinks[downloaded]);
				
				
			}


		});



	}
	
	void fetchComment(string url)
	{
		bool extracted = false;
		
		UpdateMessages("Fetching Comments");
		UnityHTTP.Request someRequest = new UnityHTTP.Request( "get", url );
		someRequest.Send( ( request ) => {
			
			UpdateMessages("Found Comments");
			JSONObject data = new JSONObject(request.response.Text);
			
			foreach(JSONObject thisPost in data.list)
			{
				//check if relevant fields exists in the json
				if (thisPost.HasField("data") )
				{
					JSONObject commentData = thisPost["data"];
					if (commentData.HasField("children"))
					{
						JSONObject commentChild = commentData["children"];

						foreach (JSONObject child in commentChild)
						{
							if (child.HasField("data"))
							{

								JSONObject childData = child["data"];
								if (childData.HasField("body"))
								{
									//finally reached the point where we can grab the stuff we need
									string thisCommet = childData.GetField("body").str;
									if (isInFilter(thisCommet.ToLower())) //check if comment contains things we are interested in
									{
										UpdateConsoleMessages("Last Comment:"+thisCommet); //tell user what we fetched
										AddComment(thisCommet);

									}

								}
								
							
							}
							
						}
						
						
						
						
					}
					

				}
				
			}
			

			if (downloaded < postLinks.Count-1 && !extracted)
			{
				extracted = true;
				downloaded++;

				UpdateMessages("Next post " + downloaded + " Waiting....");
				progressImageMain.fillAmount = (float) downloaded / postLinks.Count ;
				
				//wait because we don't want to trash reddit servers now do we?!?! do we ?
				StartCoroutine(WaitAndFetchNext(postLinks[downloaded]));

			}
			else
			{
			
				UpdateMessages("Writting file");

				WriteComments();
			}




		});



	}
	
	
	

	
	private IEnumerator WaitAndFetchNext(string url)
	{
		//wait because we don't want to trash reddit servers now do we?!?! do we ?
		yield return new WaitForSeconds(2.5f);
		
		fetchComment (url);

	}

    #endregion

    #region Helpers

    

    public void DeleteItemAt(int index, bool isAcceptPanel, GameObject tagItem)
    {
	    
	    if (isAcceptPanel)
	    { 
		    if (wordFilterAccept.Count() > index)
		    {
			    wordFilterAccept.RemoveAt(index);

			    tagItem.transform.parent = gameObject.transform; // temporary fix
			    Destroy(tagItem);

			    FixAcceptedTagIndexes();
		    }
	    }
	    else
	    {
		    if (wordFilterDeny.Count() > index)
		    {
			    wordFilterDeny.RemoveAt(index);

			    tagItem.transform.parent = gameObject.transform; // temporary fix
			    Destroy(tagItem);

			    FixDenyTagIndexes();
		    }
	    }
	    
    }

    public void AddNewTag(bool isAcceptPanel)
    {
	    if (isAcceptPanel)
	    {
		    TagItemObject newtag  = Instantiate(tagPrefab, acceptPanel.transform, true);
		    newtag.transform.localScale = new Vector3(1,1,1);
		    newtag.worker = this;
		    newtag.isAcceptPanel = isAcceptPanel;
		    newtag.index = 0;
		    newtag.tagName.text = "New Tag";
		    
		    wordFilterAccept.Insert(0, "New Tag");
		    FixAcceptedTagIndexes();
	    }
	    else
	    {
		    TagItemObject newtag  = Instantiate(tagPrefab, denyPanel.transform, true);
		    newtag.transform.localScale = new Vector3(1,1,1);
		    newtag.worker = this;
		    newtag.isAcceptPanel = isAcceptPanel;
		    newtag.index = 0;
		    newtag.tagName.text = "New Tag";
		    
		    wordFilterDeny.Insert(0, "New Tag");
		    FixDenyTagIndexes();
	    }
    }

    public void ReloadArray(int index, bool isAcceptPanel, string text)
    {
	    if (isAcceptPanel)
	    {
		    wordFilterAccept[index] = text;
	    }
	    else
	    {
		    wordFilterDeny[index] = text;
	    }
    }
    void FixAcceptedTagIndexes()
    {
	    int index = 0;
	    foreach (Transform child in acceptPanel.transform) 
	    {
		    if (child.gameObject.GetComponent<TagItemObject>() != null && wordFilterAccept.Count() > index) 
		    {
			    child.gameObject.GetComponent<TagItemObject>().index = index;
			    child.gameObject.GetComponent<TagItemObject>().tagName.text = wordFilterAccept[index];
			    index++;
	    
		    }
		    
		    
	    }



    } 
    void FixDenyTagIndexes()
    {
	    int index = 0;
	    foreach (Transform child in denyPanel.transform) 
	    {
		    if (child.gameObject.GetComponent<TagItemObject>() != null && wordFilterDeny.Count() > index) 
		    {
			    child.gameObject.GetComponent<TagItemObject>().index = index;
			    child.gameObject.GetComponent<TagItemObject>().tagName.text = wordFilterDeny[index];
			    index++;
	    
		    }
		    
		    
	    }



    }

    
    bool CommentIsClean(string comment)
    {
	    if (minCharCount == 0)
	    {
		    minCharCount = 3;
	    }

	    if (maxCharCount == 0)
	    {
		    maxCharCount = 300;
	    }
	    if (comment.Length < minCharCount)
	    {
		    return false;
	    }
	    else if (comment.Length > maxCharCount)
	    {
		    return false;
	    }
	    else if (comment.Contains("\\"))
	    {
		    return false;
	    }
	    else if (comment.Contains("..."))
	    {
		    return false;
	    }
	    else if (comment.Contains("\\u"))
	    {
		    return false;
	    }

	    foreach (string nonoWords in wordFilterDeny)
	    {
		    if (comment.Contains(nonoWords))
		    {
			    return false;
		    }
	    }
		
	    // foreach (CommentsClass commentClass in comments)
	    // {
		   //  if (commentClass.comments.Contains(comment))
		   //  {
			  //   Debug.Log("Blocked "+ comment);
			  //   return false;
		   //  }
		   //
	    // }


	    return true;

    }

    
    void AddComment(string comment)
    {
		
	    string commentId = GetCommentId(comment.ToLower());
	    //int commentClassNum = 0;
	    if (commentId != null)
	    {
		    foreach (CommentsClass thisComment in comments)
		    {
			    if (thisComment.commentId.Equals(commentId))
			    {
				    thisComment.comments.Add(comment);
			    }
		    }
			
	    }


    }

    
    string GetCommentId(string comment)
    {
	    foreach (string filter in wordFilterAccept)
	    {
		    if (comment.Contains(filter))
		    {
			    return filter;
		    }
			
	    }

	    return null;
    }
    bool isInFilter(string comment)
    {
	    foreach (string filter in wordFilterAccept)
	    {
		    if (comment.Contains(filter))
		    {
			    return true;
		    }
			
	    }
	    return false;
    }


    #endregion





    #region I/O

    
    void WriteComments()
    {
	    string path = Application.persistentDataPath + "/comments.txt";
	    if (!File.Exists(path)) {
		    File.WriteAllText(path, "");
	    }

	    int tagNum = 0;
	    foreach (CommentsClass commentClass in comments)
	    {

		    int lineNum = 0;
		    foreach (string comment in commentClass.comments)
		    {
			    if (CommentIsClean(comment) && CommentIsClean(comment.ToLower()))
			    {
				    File.AppendAllText(path, commentClass.commentId + ": \n");

				    if (comment.EndsWith(".") || comment.EndsWith("!"))
				    {
					    File.AppendAllText(path, comment + " \n\n");
				    }
				    else
				    {
					    File.AppendAllText(path, comment + ". \n\n");
				    }
					
			    }
			    else
			    {
				    UpdateConsoleMessages("Blocked: "+comment);
				    Debug.Log("Blocked");
			    }

			    lineNum++;
				
				
			    UpdateMessages("TAg Num " + tagNum + " Line Num " + lineNum);
		    }
		    tagNum++;
			
	    }
		
	    UpdateMessages("Saved In:"+Application.persistentDataPath + "/comments.txt");
	    progressImageMain.fillAmount = 1;
	    UpdateConsoleMessages("Done");
	    
	    //EditorUtility.RevealInFinder(Application.persistentDataPath);
	    submit.transform.GetChild(0).GetComponent<Text>().text = "Restart App";

    }

    #endregion

	

	/// <summary>
	/// Will hold all the comments followed by the tags for them
	/// </summary>
	[Serializable]
	public class CommentsClass
	{
		public string commentId;
		public List<string> comments = new List<string>();
	}
	
	

	

	public void reset()
	{
		 SceneManager.LoadScene("u");
	}
	public void exit()
	{
		Application.Quit();
	}

}
