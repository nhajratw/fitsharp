// Copyright � 2011 Syterra Software Inc. All rights reserved.
// The use and distribution terms for this software are covered by the Common Public License 1.0 (http://opensource.org/licenses/cpl.php)
// which can be found in the file license.txt at the root of this distribution. By using this software in any fashion, you are agreeing
// to be bound by the terms of this license. You must not remove this notice, or any other, from this software.

using fitSharp.Fit.Engine;
using fitSharp.Fit.Model;
using fitSharp.Fit.Operators;
using fitSharp.Machine.Model;

namespace fitSharp.Fit.Service {
    public interface CellOperation {
        void Check(object systemUnderTest, Tree<Cell> memberName, Tree<Cell> parameters, Tree<Cell> expectedCell);
        void Check(object systemUnderTest, TypedValue actualValue, Tree<Cell> expectedCell);
        bool Compare(TypedValue actual, Tree<Cell> expectedCell);
        void Input(object systemUnderTest, Tree<Cell> memberName, Tree<Cell> cell);
        TypedValue TryInvoke(object target, Tree<Cell> memberName, Tree<Cell> parameters, Tree<Cell> targetCell);
    }

    public static class CellOperationExtension {
        public static void Check(this CellOperation operation, object systemUnderTest, Tree<Cell> memberName, Tree<Cell> expectedCell) {
            operation.Check(systemUnderTest, memberName, new CellTree(), expectedCell);
        }

        public static TypedValue Invoke(this CellOperation operation, object target, Tree<Cell> memberName) {
            return operation.Invoke(target, memberName, new CellTree());
        }

        public static TypedValue Invoke(this CellOperation operation, object target, Tree<Cell> memberName, Tree<Cell> parameters) {
            TypedValue result = operation.TryInvoke(target, memberName, parameters);
            result.ThrowExceptionIfNotValid();
            return result;
        }

        public static TypedValue Invoke(this CellOperation operation, object target, Tree<Cell> memberName, Tree<Cell> parameters, Tree<Cell> targetCell) {
            TypedValue result = operation.TryInvoke(target, memberName, parameters, targetCell);
            result.ThrowExceptionIfNotValid();
            return result;
        }

        public static TypedValue TryInvoke(this CellOperation operation, object target, Tree<Cell> memberName) {
            return operation.TryInvoke(target, memberName, new CellTree());
        }

        public static TypedValue TryInvoke(this CellOperation operation, object target, Tree<Cell> memberName, Tree<Cell> parameters) {
            return operation.TryInvoke(target, memberName, parameters, null);
        }

    }

    public class CellOperationImpl: CellOperation {
        private readonly CellProcessor processor;

        public CellOperationImpl(CellProcessor processor) {
            this.processor = processor;
        }

        public void Check(object systemUnderTest, Tree<Cell> memberName, Tree<Cell> parameters, Tree<Cell> expectedCell) {
            processor.Invoke(
                ExecuteContext.Make(ExecuteCommand.Check, systemUnderTest),
                ExecuteContext.CheckCommand,
                ExecuteParameters.Make(memberName, parameters, expectedCell));
        }

        public void Check(object systemUnderTest, TypedValue actualValue, Tree<Cell> expectedCell) {
            processor.Invoke(
                ExecuteContext.Make(ExecuteCommand.Check, systemUnderTest, actualValue),
                ExecuteContext.CheckCommand,
                ExecuteParameters.Make(expectedCell));
        }

        public TypedValue TryInvoke(object target, Tree<Cell> memberName, Tree<Cell> parameters, Tree<Cell> targetCell) {
            return processor.Invoke(
                ExecuteContext.Make(ExecuteCommand.Invoke, new TypedValue(target)), 
                ExecuteContext.InvokeCommand,
                ExecuteParameters.Make(memberName, parameters, targetCell));
        }

        public bool Compare(TypedValue actual, Tree<Cell> expectedCell) {
            return processor.Invoke(
                    new TypedValue(new CompareOperation(processor, actual, expectedCell)), 
                    "Do",
                    new CellTree())
                .GetValue<bool>();
        }

        public void Input(object systemUnderTest, Tree<Cell> memberName, Tree<Cell> inputCell) {
            processor.Invoke(
                    new TypedValue(new InputOperation(processor, systemUnderTest, memberName, inputCell)), 
                    "Do",
                    new CellTree());
        }
    }
}
