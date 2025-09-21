using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public class BootstrapPlayMode : MonoBehaviour {
    private const string bootstrapScenePath = "Assets/Scenes/Bootstrap.unity";
    private const string lastSceneKey = "BootstrapPlayMode_LastScene";

    static BootstrapPlayMode() {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    private static void OnPlayModeChanged(PlayModeStateChange state) {
        if (state == PlayModeStateChange.ExitingEditMode) {
            var activeScene = EditorSceneManager.GetActiveScene();

            // If scene has unsaved changes, ask what to do.
            if (activeScene.isDirty) {
                int choice = EditorUtility.DisplayDialogComplex(
                    "Unsaved Scene Changes",
                    $"The scene \"{activeScene.name}\" has unsaved changes.\n\nWhat would you like to do?",
                    "Save and Play",
                    "Cancel Play",
                    "Don't Save"
                );

                if (choice == 0) // Save and Play
                {
                    if (string.IsNullOrEmpty(activeScene.path)) {
                        // Untitled scene: ask for a save path
                        string savePath = EditorUtility.SaveFilePanelInProject(
                            "Save Scene",
                            string.IsNullOrEmpty(activeScene.name) ? "New Scene" : activeScene.name,
                            "unity",
                            "Choose a location to save the current scene."
                        );

                        if (string.IsNullOrEmpty(savePath)) {
                            EditorApplication.isPlaying = false;
                            return;
                        }

                        if (!EditorSceneManager.SaveScene(activeScene, savePath)) {
                            EditorUtility.DisplayDialog("Save Failed",
                                "Unity couldn't save the scene. Play Mode will be cancelled.", "OK");
                            EditorApplication.isPlaying = false;
                            return;
                        }
                    } else {
                        if (!EditorSceneManager.SaveScene(activeScene)) {
                            EditorUtility.DisplayDialog("Save Failed",
                                "Unity couldn't save the scene. Play Mode will be cancelled.", "OK");
                            EditorApplication.isPlaying = false;
                            return;
                        }
                    }
                } else if (choice == 1) // Cancel Play
                  {
                    EditorApplication.isPlaying = false;
                    return;
                } else {
                    EditorUtility.DisplayDialog(
                        "Warning",
                        "Proceeding without saving. Any unsaved changes in this scene will be lost.",
                        "OK");
                }
            }

            // Record the scene we were in (after any save)
            var currentPath = EditorSceneManager.GetActiveScene().path;
            EditorPrefs.SetString(lastSceneKey, currentPath);

            // Swap to bootstrap scene (only if it exists)
            if (!string.IsNullOrEmpty(bootstrapScenePath) && File.Exists(bootstrapScenePath)) {
                EditorSceneManager.OpenScene(bootstrapScenePath);
            } else {
                EditorUtility.DisplayDialog(
                    "Bootstrap Scene Missing",
                    $"Could not find:\n{bootstrapScenePath}\n\nPlay Mode will continue in the current scene.",
                    "OK");
            }
        } else if (state == PlayModeStateChange.EnteredEditMode) {
            // Return to the scene we were editing
            var lastScene = EditorPrefs.GetString(lastSceneKey, string.Empty);
            if (!string.IsNullOrEmpty(lastScene) && File.Exists(lastScene)) {
                // Guard: avoid reopening the same scene unnecessarily
                if (EditorSceneManager.GetActiveScene().path != lastScene) {
                    EditorSceneManager.OpenScene(lastScene);
                }
            }
            // else: silently do nothing (scene was deleted/moved)
        }
    }
}
