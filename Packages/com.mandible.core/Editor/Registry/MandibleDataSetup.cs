using UnityEditor;
using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Mandible.Registry
{
    public static partial class MandibleData
    {
        private const string DefaultFolderName = "";

        public static Action onDataCreated; 
        public static Action onDataUpdated;
        public static Action onDataRepaired;

        public static string Setup()
        {
            //If Mandible folder already exists
            TryGetRoot(out var root, out var path);
            if (root != null && IsFolderStructureValid())
            {
                int choice = EditorUtility.DisplayDialogComplex(
                    "Mandible Data Root Found",
                    $"A MandibleDataRoot asset was found at:\n{root}\nWhat would you like to do?",
                    "Update",
                    "Repair",
                    "Leave"
                );

                switch (choice)
                {
                    case 0: 
                        Update(root); //Update
                        break;
                    case 1: 
                        Repair(root); //Repair
                        break;  
                    default:
                        //Leave
                        break;
                };

                return path;
            }

            //If Mandible folder needs repair
            if(root != null && !IsFolderStructureValid())
            {
                if(EditorUtility.DisplayDialog(
                    "Mandible Folder Structure Invalid",
                    "The Mandible folder structure is invalid. It will be repaired automatically.",
                    "OK"
                ))
                {
                    Repair(root);
                    return path;
                }
            }

            //If Mandible folder doesn't exist
            string baseFolder = EditorUtility.DisplayDialogComplex(
            "Mandible Setup",
            "Do you want to create the Mandible folder at the default location?",
            "Yes",
            "No",
            "Cancel"
            ) switch
            {
                0 => "Assets/" + DefaultFolderName,     // Default
                1 => AskCustomFolderPath(),             // Custom
                _ => null                               // Cancel
            };
            if(string.IsNullOrEmpty(baseFolder)) return null;

            string mandibleFolder = baseFolder + "/Mandible";

            // Ensure folder exists in AssetDatabase
            CreateFolderIfMissing(mandibleFolder);

            // Ensure MandibleDataRoot exists
            string rootAssetPath = Path.Combine(mandibleFolder, RootAssetName);
            if (!File.Exists(rootAssetPath))
            {
                var newRoot = ScriptableObject.CreateInstance<MandibleDataRoot>();
                AssetDatabase.CreateAsset(newRoot, rootAssetPath);
                AssetDatabase.SetLabels(newRoot, new[] { RootLabel });
                AssetDatabase.SaveAssets();
                Debug.Log($"Created MandibleDataRoot.asset at {rootAssetPath}");
            }
            else
            {
                Debug.Log("MandibleDataRoot.asset already exists.");
            }

            onDataCreated?.Invoke();

            return mandibleFolder;
        }

        public static void Update(MandibleDataRoot root)
        {
            //Unsure for now if there's anything that needs to be updated
            onDataUpdated?.Invoke();
        }

        public static void Repair(MandibleDataRoot root)
        {
            IsFolderStructureValid(out var missingFolders);
            if(missingFolders.Length == 0) return;
            
            //Repair
            foreach (var folder in missingFolders)
            {
                CreateFolder(folder);
                Debug.Log($"Created missing folder: {folder}");
            }

            onDataRepaired?.Invoke();
        }

        public static string AskCustomFolderPath()
        {
            while(true)
            {
                string folderName = EditorUtility.SaveFolderPanel(
                    "Select Root Folder for Mandible Data",
                    Application.dataPath,
                    DefaultFolderName
                );
                
                // If user cancels
                if(string.IsNullOrEmpty(folderName))
                {
                    if (EditorUtility.DisplayDialog(
                        "Cancel Setup",
                        "No folder selected. Do you want to cancel Mandible setup?",
                        "Yes",
                        "No"
                    ))
                    {
                        return null; // User chose to cancel
                    }
                    continue; // User wants to try again
                }

                // If not in Assets folder
                if (!folderName.StartsWith(Application.dataPath))
                {
                    EditorUtility.DisplayDialog(
                        "Invalid Folder",
                        "The Mandible folder must be inside the project's Assets folder. Please select a folder inside Assets.",
                        "OK"
                    );
                    continue; // Try again
                }

                string relativePath = "Assets" + folderName.Substring(Application.dataPath.Length);
                return relativePath; // Valid folder selected
            }
        }

        private static void CreateFolderIfMissing(string relativePath)
        {
            if (AssetDatabase.IsValidFolder(relativePath)) return;
            
            CreateFolder(relativePath);
            CreateFolder(relativePath + "/Data"); //Data folder is required for external Mandible assets that have custom registries
        }
        
        private static void CreateFolder(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath)) return;

            string[] parts = relativePath.Split('/');

            string currentPath = parts[0];

            for (int i = 1; i < parts.Length; i++)
            {
                string nextPath = Path.Combine(currentPath, parts[i]).Replace("\\", "/");
                if (!AssetDatabase.IsValidFolder(nextPath))
                {
                    AssetDatabase.CreateFolder(currentPath, parts[i]);
                }
                currentPath = nextPath;
            }
        }

        //Helpers

        public static bool IsFolderStructureValid(out string[] missingFolders)
        {
            string mandibleRoot = "Assets/" + DefaultFolderName + "/Mandible";

            // Define required subfolders
            string[] requiredFolders = new string[]
            {
                mandibleRoot,
                mandibleRoot + "/Data"
            };

            var missing = new System.Collections.Generic.List<string>();

            foreach (string folder in requiredFolders)
            {
                if (!AssetDatabase.IsValidFolder(folder))
                    missing.Add(folder);
            }

            missingFolders = missing.ToArray();
            return missingFolders.Length == 0;
        }

        public static bool IsFolderStructureValid()
        {
            return IsFolderStructureValid(out _);
        }
    }
}
