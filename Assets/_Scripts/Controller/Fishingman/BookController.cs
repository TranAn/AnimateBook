﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Fungus;

public class BookController : MonoBehaviour {
	private List<GameObject> loadedPages = new List<GameObject>();
	public GameObject container;
	public GameObject placeHolder;
	public GameObject placeHolderLeft;
	public GameObject[] pages;
	public Image background;
	public Sprite[] bgResources;

	public Flowchart flowChart;
	private Block block;

	private Dictionary<string, int> commandNames =
		new Dictionary<string, int>();

	private int curLoadedPageNumber = 0;
	private int nextLoadedPageNumber = 0;

	private int beginPageNumber = 0;	//index of page in pages
	private int endPageNumber = 0;	//index of page in pages
	private int curPageNumber = 0;	//index of page in pages

	private GameObject curPage;
	private GameObject nextPage;

	private Animation animation;
	private Animation curPageAnimation;
	private Animation nextPageAnimation;

	private const int MAX_PAGE = 4;

	private bool animationAvailable;

	// Use this for initialization
	void Start () {
		animationAvailable = false;
		curLoadedPageNumber = -1;
		nextLoadedPageNumber = 0;

		commandNames.Add("page1", 5);
		commandNames.Add("page2", 12);
		commandNames.Add("page3", 25);
		commandNames.Add("page4", 32);
		commandNames.Add("page5", 40);
		commandNames.Add("page6", 48);
		commandNames.Add("page7", 57);
		commandNames.Add("page8", 67);
		commandNames.Add("page9", 76);


		block = flowChart.FindBlock("Start");

		StartCoroutine (LoadPageIntoContainers ());

		animationAvailable = true;

		if (loadedPages.Count > 0) {
			animation = GetComponent<Animation>();
		}
	}

	IEnumerator LoadPageIntoContainers () {
		yield return new WaitForSeconds (3.5f);	//waiting for the book is opened

		beginPageNumber = 0;
		endPageNumber = MAX_PAGE - 1;

		for (int count = beginPageNumber; count <= endPageNumber; count++) {

			string objName = "Page" + count + ".prefab";
			Debug.Log("Page :: " + objName );

			loadedPages.Add(Instantiate(pages[count], placeHolder.transform.position, placeHolder.transform.rotation, container.transform));
		}
	}

	// Update is called once per frame
	void Update () {
		//open next page
		if (SwipeManager.instance.IsSwiping (SwipeDirection.Left)) {

			swipeLeftHandle ();
		}

		//back page
		if (SwipeManager.instance.IsSwiping (SwipeDirection.Right)) {

			swipeRightHandle ();
		}
	}

	public void swipeLeftHandle () {
		Debug.Log("curLoadedPageNumber :: " + curLoadedPageNumber );
		Debug.Log("nextLoadedPageNumber :: " + nextLoadedPageNumber );

		if (animationAvailable == true) {
			animationAvailable = false;

			//close right items on current page, open next page
			//make the next page as current page, increase next page number, open items on new next page
			if (curLoadedPageNumber >= 0 && curLoadedPageNumber < loadedPages.Count) {
				string clip = "CloseRightItemsPage";
				Debug.Log ("CloseRightItemsPage :: " + clip);

				playAnimationCurrentPage (clip);
			}

			//close left items and open right items on current page
			if (nextLoadedPageNumber >= 0 && nextLoadedPageNumber < loadedPages.Count) {
				string clip = "OpenPage";
				Debug.Log ("OpenPage :: " + clip);

				playAnimationNextPage (clip);

				curLoadedPageNumber = nextLoadedPageNumber;
				curPageNumber ++;

				string command = "page" + curPageNumber;
				Debug.Log ("command :: " + command);
				int commandIndex = 0;
				bool b = commandNames.TryGetValue(command, out commandIndex);
				Debug.Log ("commandIndex :: " + commandIndex);
				block.JumpToCommandIndex = commandIndex;
				block.Execute();
			}

			//open left items on new next page
			if (nextLoadedPageNumber < loadedPages.Count - 1) {
				nextLoadedPageNumber += 1;

				string clip = "OpenLeftItemsPage";
				Debug.Log("OpenLeftItemsPage :: " + clip );

				playAnimationNextPage (clip);

			} else {
				nextLoadedPageNumber = loadedPages.Count;
			}


			//load more content
			if (curLoadedPageNumber == 2) {
				//load more content from pages and add to the end of loadedPages
				//if there is no more content, do nothing
				if (endPageNumber < pages.Length - 1) {
					endPageNumber ++;
					GameObject tmp = (GameObject)pages[endPageNumber];

					loadedPages.Add(Instantiate(tmp, placeHolder.transform.position, placeHolder.transform.rotation, container.transform));

					beginPageNumber ++;
					tmp = (GameObject)loadedPages[0];
					Destroy(tmp);
					loadedPages.RemoveAt(0);

					curLoadedPageNumber = 1;
					nextLoadedPageNumber = 2;
				}
			}
		}
	}

	public void swipeRightHandle () {

		if (animationAvailable == true) {
			animationAvailable = false;
			Debug.Log("curLoadedPageNumber :: " + curLoadedPageNumber );
			Debug.Log("nextLoadedPageNumber :: " + nextLoadedPageNumber );

			//close left items on next page then close current page (call backpage on current page) 
			//set previous page as current page, open right items of it
			if (nextLoadedPageNumber >= 0 && nextLoadedPageNumber < loadedPages.Count) {
				//close left items on next page
				string clip = "CloseLeftItemsPage";
				Debug.Log ("CloseLeftItemsPage :: " + clip);

				playAnimationNextPage (clip);
			}

			//close current page
			if (curLoadedPageNumber >= 0 && curLoadedPageNumber < loadedPages.Count) {
				string clip = "BackPage";
				Debug.Log("BackPage :: " + clip );

				playAnimationCurrentPage(clip);

				nextLoadedPageNumber = curLoadedPageNumber;
				curPageNumber --;

				string command = "page" + curPageNumber;
				Debug.Log ("command :: " + command);
				int commandIndex = 0;
				bool b = commandNames.TryGetValue(command, out commandIndex);
				Debug.Log ("commandIndex :: " + commandIndex);

				block.JumpToCommandIndex = commandIndex;
				block.Execute();
			}

			if (curLoadedPageNumber > 0) {
				curLoadedPageNumber -= 1;

				//open right items
				string clip = "OpenRightItemsPage";
				Debug.Log("curLoadedPageNumber :: " + curLoadedPageNumber );
				playAnimationCurrentPage(clip);

			} else {
				curLoadedPageNumber = -1;
			}

			//load previous page
			if (curLoadedPageNumber == 0) {
				//load more previous content from pages and add to the begin of loadedPages
				//if there is no more content, do nothing
				if (beginPageNumber > 0) {
					beginPageNumber --;
					GameObject tmp = (GameObject)pages[beginPageNumber];

					loadedPages.Insert(0, Instantiate(tmp, placeHolderLeft.transform.position, placeHolderLeft.transform.rotation, container.transform));

					endPageNumber --;
					tmp = (GameObject)loadedPages[loadedPages.Count - 1];
					Destroy(tmp);
					loadedPages.RemoveAt(loadedPages.Count - 1);

					curLoadedPageNumber = 1;
					nextLoadedPageNumber = 2;
				}
			}
		}
	}

	public void resetAnimationFlag () {
		Debug.Log("resetAnimationFlag");
		animationAvailable = true;
	}
		
	public void changeBackgroundImage (int sceneID) {
		if (bgResources.Length > 0 && bgResources.Length > sceneID) {
			
			background.sprite = bgResources[sceneID];
		}
	}

	void playAnimationCurrentPage (string clip) {
		getAnimationCurrentPage();

		if (curPageAnimation) {
			curPageAnimation.Play(clip);
		}
	}

	void playAnimationNextPage (string clip) {
		getAnimationNextPage();

		if (nextPageAnimation) {
			nextPageAnimation.Play(clip);
		}
	}

	void getAnimationCurrentPage () {
		if (curLoadedPageNumber >= 0 && curLoadedPageNumber < loadedPages.Count) {
			curPage = loadedPages[curLoadedPageNumber];
			curPageAnimation = curPage.GetComponent<Animation>();

		} else {
			curPageAnimation = null;
		}
	}

	void getAnimationNextPage () {
		if (nextLoadedPageNumber >= 0 && nextLoadedPageNumber < loadedPages.Count) {
			nextPage = loadedPages[nextLoadedPageNumber];
			nextPageAnimation = nextPage.GetComponent<Animation>();

		} else {
			nextPageAnimation = null;
		}
	}

	string clipIndexToName(int index)
	{
		AnimationClip clip = getClipByIndex(index);
		if (clip == null)
			return null;
		return clip.name;
	}
	AnimationClip getClipByIndex(int index)
	{
		int i = 0;
		foreach (AnimationState animationState in animation)
		{
			if (i == index)
				return animationState.clip;
			i++;
		}
		return null;
	}

	AnimationClip getClipByName(string specificName)
	{
		string name = "";
		foreach (AnimationState animationState in animation)
		{
			if (animationState.clip.name == specificName)
				return animationState.clip;
		}
		return null;
	}
}
