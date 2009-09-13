// Copyright � 2009 Syterra Software Inc.
// This program is free software; you can redistribute it and/or modify it under the terms of the GNU General Public License version 2.
// This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.

using System.IO;
using fitSharp.Fit.Model;
using fitnesse.fitserver;
using fitSharp.IO;
using fitSharp.Machine.Application;
using fitSharp.Machine.Model;

namespace fit.Runner {
    public class StoryTestFile: StoryTestPage {
        private readonly Configuration configuration;
        private readonly StoryFileName myPath;
        private readonly StoryTestFolder myFolder;
        private readonly FolderModel myFolderModel;
        private ElapsedTime elapsedTime;
        private ResultWriter resultWriter;
        private TestStatusHandler handler;
        private string myContent;
        private Parse myTables;

        public StoryTestFile(Configuration configuration, string thePath, StoryTestFolder theFolder, FolderModel theFolderModel) {
            this.configuration = configuration;
            myPath = new StoryFileName(thePath);
            myFolder = theFolder;
            myFolderModel = theFolderModel;
        }

        public string Name { get { return Path.GetFileName(myPath.Name); }}

        public void ExecuteStoryPage(ResultWriter resultWriter, TestStatusHandler handler) {
            elapsedTime = new ElapsedTime();
            this.resultWriter = resultWriter;
            this.handler = handler;
            if (IsTest) {
                new StoryTest(Tables, WriteFile).Execute();
                return;
            }
            if (myPath.IsSuiteSetUp) {
                new StoryTest(RawTables, WriteFile).Execute();
                return;
            }
            CopyFile();
            handler(new TestStatus());
        }

        private void WriteFile(Tree<Cell> theTables, TestStatus status) {
            var tables = (Parse) theTables.Value;
            WriteResult(tables, status, elapsedTime);
            var pageResult = new PageResult(myPath.Name);
            pageResult.Append(theTables.ToString());
            pageResult.TestStatus = status;
            resultWriter.WritePageResult(pageResult);
            handler(status);
        }

        public bool IsTest {
            get {
                if (myPath.IsSetUp || myPath.IsTearDown || myPath.IsSuiteSetUp) return false;
                return (RawTables != null);
            }
        }

        public string Content {
            get {
                if (myContent == null) myContent = myFolderModel.FileContent(myPath.Name);
                return myContent;
            }
        }

        private void CopyFile() {
            myFolderModel.CopyFile(myPath.Name, OutputPath);
        }

        private void WriteResult(Cell theTables, TestStatus status, ElapsedTime elapsedTime) {
            string outputFile = OutputPath;
            var output = new StringWriter();
            output.Write(configuration.GetItem<Service.Service>().Parse(typeof(StoryTestString), TypedValue.Void, new TreeLeaf<Cell>(theTables)).ValueString);
            output.Close();
            myFolderModel.MakeFile(outputFile, output.ToString());
            myFolder.ListFile(outputFile, status, elapsedTime);
        }

        private Parse Tables {
            get {
                return myFolder.Decoration.IsEmpty
                           ? RawTables
                           : Parse(myFolder.Decoration.Decorate(Content));
            }
        }

        private Parse RawTables {
            get {
                if (myTables == null) {
                    FitVersionFixture.Reset();
                    myTables = Parse(Content);
                }
                return myTables;
            }
        }

        private Parse Parse(string content) {
            Tree<Cell> result = configuration.GetItem<Service.Service>().Compose(new TypedValue(new StoryTestString(content)));
            return result != null ? (Parse)result.Value : null;
        }

        private string OutputPath {
            get {
                return Path.Combine(myFolder.OutputPath, Path.GetFileName(myPath.Name));
            }
        }
    }

    public class StoryFileName {
        public StoryFileName(string theName) {
            myName = theName;
        }

        public string Name { get { return myName; }}

        public bool IsSetUp {
            get {
                string name = Path.GetFileName(myName);
                return ourSetupIdentifier1.Equals(name) || ourSetupIdentifier2.Equals(name);
            }
        }

        public bool IsSuiteSetUp {
            get {
                string name = Path.GetFileName(myName);
                return ourSuiteSetupIdentifier1.Equals(name) || ourSuiteSetupIdentifier2.Equals(name);
            }
        }

        public bool IsTearDown {
            get {
                string name = Path.GetFileName(myName);
                return ourTeardownIdentifier1.Equals(name) || ourTeardownIdentifier2.Equals(name);
            }
        }

        private static readonly IdentifierName ourSetupIdentifier1 = new IdentifierName("setup.html");
        private static readonly IdentifierName ourSetupIdentifier2 = new IdentifierName("setup.htm");
        private static readonly IdentifierName ourTeardownIdentifier1 = new IdentifierName("teardown.html");
        private static readonly IdentifierName ourTeardownIdentifier2 = new IdentifierName("teardown.htm");
        private static readonly IdentifierName ourSuiteSetupIdentifier1 = new IdentifierName("suitesetup.html");
        private static readonly IdentifierName ourSuiteSetupIdentifier2 = new IdentifierName("suitesetup.htm");
        private readonly string myName;
    }
}