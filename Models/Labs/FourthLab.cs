﻿using System.Linq;
using System.Security.Cryptography;
using ComputedMath.Solvers.FourthLab;
using ComputedMath.Solvers.SecondLab;
using Irony.Interpreter;
using Irony.Parsing;

namespace ComputedMath.Models.Labs {
    public class FourthLab : LabResultsModel {
        private readonly LanguageData _languageData;
        private readonly ScriptApp _scriptApp;
        private ParseTree _parseTree;

        public override string Name => "Differnsial equation solver";

        public string Function { get; set; } = "x ** 2";
        public double X0 { get; set; } = 1;
        public double Y0 { get; set; } = 1;
        public double H { get; set; } = 0.1;
        public int Count { get; set; } = 10;
        public double Precision { get; set; } = 0.01;

        public FourthLab() {
            var expressionGrammar = new LabGrammar();
            _languageData = new LanguageData(expressionGrammar);
            var labRuntime = new LabRuntime(_languageData);
            _scriptApp = new ScriptApp(labRuntime);
        }

        public override void Solve() {
            base.Solve();

            var parser = new Parser(_languageData);
            _parseTree = parser.Parse(Function);

            if (_parseTree.HasErrors()) {
                Results.Add(new BoxModel("Error at parsing", "Syntax errors found. Abrting."));
                return;
            }

            Results.Add(new LaTeXBox("Input", "\\frac{dy}{dx} = " + _parseTree.ToLaTeX()));

            double[] calculatedYs = DiferensialSolver.Milne(
                CalculateFunction,
                Y0,
                X0,
                Count,
                H,
                Precision
            );

            Results.Add(new ChartBoxModel("Result", new[] {
                calculatedYs.Select((y, i) => (X0 + H * i, calculatedYs[i])).ToArray()
            }));
        }

        private double CalculateFunction(double x, double y) {
            _scriptApp.Globals["x"] = x;
            _scriptApp.Globals["y"] = y;

            var res = (double) _scriptApp.Evaluate(_parseTree);

            if (double.IsNaN(res) || double.IsInfinity(res)) {
                return CalculateFunction(x + H / 2, y + H / 2);
            }

            return res;
        }
    }
}