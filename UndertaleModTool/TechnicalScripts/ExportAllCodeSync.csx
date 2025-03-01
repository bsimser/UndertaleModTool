﻿using System.Text;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

EnsureDataLoaded();

string codeFolder = GetFolder(FilePath) + "Export_Code" + Path.DirectorySeparatorChar;
ThreadLocal<GlobalDecompileContext> DECOMPILE_CONTEXT = new ThreadLocal<GlobalDecompileContext>(() => new GlobalDecompileContext(Data, false));

if (Directory.Exists(codeFolder)) 
{
    codeFolder = GetFolder(FilePath) + "Export_Code_2" + Path.DirectorySeparatorChar;
}

Directory.CreateDirectory(codeFolder);

SetProgressBar(null, "Code Entries", 0, Data.Code.Count);
StartUpdater();

int failed = 0;
await Task.Run(DumpCode);

await StopUpdater();
HideProgressBar();
ScriptMessage("Export Complete.\n\nLocation: " + codeFolder + " " + failed.ToString() + " failed");

string GetFolder(string path)
{
    return Path.GetDirectoryName(path) + Path.DirectorySeparatorChar;
}


void DumpCode() 
{
    foreach(UndertaleCode code in Data.Code)
    {
        string path = Path.Combine(codeFolder, code.Name.Content + ".gml");
        if (code.ParentEntry == null)
        {
            try 
            {
                File.WriteAllText(path, (code != null ? Decompiler.Decompile(code, DECOMPILE_CONTEXT.Value) : ""));
            }
            catch (Exception e) 
            {
                if (!(Directory.Exists(codeFolder + "/Failed/")))
                {
                    Directory.CreateDirectory(codeFolder + "/Failed/");
                }
                path = Path.Combine(codeFolder + "/Failed/", code.Name.Content + ".gml");
                File.WriteAllText(path, "/*\nDECOMPILER FAILED!\n\n" + e.ToString() + "\n*/");
                failed += 1;
            }
        }
        else
        {
            if (!(Directory.Exists(codeFolder + "/Duplicates/")))
            {
                Directory.CreateDirectory(codeFolder + "/Duplicates/");
            }
            try 
            {
                path = Path.Combine(codeFolder + "/Duplicates/", code.Name.Content + ".gml");
                File.WriteAllText(path, (code != null ? Decompiler.Decompile(code, DECOMPILE_CONTEXT.Value) : ""));
            }
            catch (Exception e) 
            {
                if (!(Directory.Exists(codeFolder + "/Duplicates/Failed/")))
                {
                    Directory.CreateDirectory(codeFolder + "/Duplicates/Failed/");
                }
                path = Path.Combine(codeFolder + "/Duplicates/Failed/", code.Name.Content + ".gml");
                File.WriteAllText(path, "/*\nDECOMPILER FAILED!\n\n" + e.ToString() + "\n*/");
                failed += 1;
            }
        }

        IncProgress();
    }
}

