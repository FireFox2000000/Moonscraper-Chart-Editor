// Copyright (c) 2016-2020 Alexander Ong
// See LICENSE in project root for license information.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;
using System.Threading;

public class LoadingTask
{
    public string description;
    public Action task;

    public LoadingTask(string description, Action task)
    {
        this.description = description;
        this.task = task;
    }
}

public class LoadingTasksManager : MonoBehaviour
{
    public LoadingScreenFader loadingScreen;
    public bool isRunningTask { get; private set; }

    private void Start()
    {
        loadingScreen.gameObject.SetActive(true);   // This is initially hidden so we can actually see things in the editor.
    }

    public async void KickTasks(IList<LoadingTask> tasks)
    {
        Debug.Assert(!isRunningTask);

        isRunningTask = true;

        ChartEditor.Instance.ChangeStateToLoading();
        loadingScreen.FadeIn();

        try {
            for (int i = 0; i < tasks.Count; ++i)
            {
                LoadingTask currentTask = tasks[i];
                loadingScreen.loadingInformation.text = currentTask.description;

                await Task.Run(currentTask.task);
            }
        } finally {
            isRunningTask = false;

            loadingScreen.loadingInformation.text = "Complete!";
            loadingScreen.FadeOut();

            ChartEditor.Instance.ChangeStateToEditor();
        }
    }
}
