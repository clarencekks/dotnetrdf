/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Writing;

namespace VDS.RDF.Test.Parsing
{
    [TestClass]
    public class LangSpecTests
    {
        private IGraph _original;
        private TripleStore _store;

        private void EnsureTestData(String file)
        {
            if (this._original == null)
            {
                Graph g = new Graph();
                g.Assert(g.CreateBlankNode(), g.CreateUriNode(UriFactory.Create("http://example.org/predicate")), g.CreateLiteralNode("literal", "en-123"));
                g.Assert(g.CreateBlankNode(), g.CreateUriNode(UriFactory.Create("http://example.org/predicate")), g.CreateLiteralNode("literal", "en-gb-us"));
                g.Assert(g.CreateBlankNode(), g.CreateUriNode(UriFactory.Create("http://example.org/predicate")), g.CreateLiteralNode("literal", "en-123-abc"));
                this._original = g;

                this._store = new TripleStore();
                this._store.Add(this._original);
            }

            if (!File.Exists(file))
            {
                MimeTypeDefinition def = MimeTypesHelper.GetDefinitions(MimeTypesHelper.GetMimeTypes(Path.GetExtension(file))).FirstOrDefault();
                if (def != null)
                {
                    if (def.CanWriteRdf)
                    {
                        this._original.SaveToFile(file);
                    }
                    else if (def.CanWriteRdfDatasets)
                    {
                        this._store.SaveToFile(file);
                    }
                    else
                    {
                        Assert.Fail("Unable to ensure test data");
                    }
                }
                else
                {
                    Assert.Fail("Unsupported file type");
                }
            }
            else
            {
                Assert.Fail("Unable to ensure test data");
            }
        }

        private void TestLangSpecParsing(String file)
        {
            this.EnsureTestData(file);

            MimeTypeDefinition def = MimeTypesHelper.GetDefinitions(MimeTypesHelper.GetMimeTypes(Path.GetExtension(file))).FirstOrDefault();
            if (def != null)
            {
                if (def.CanParseRdf)
                {
                    Graph g = new Graph();
                    g.LoadFromFile(file);

                    Assert.AreEqual(this._original, g);
                }
                else if (def.CanParseRdfDatasets)
                {
                    TripleStore store = new TripleStore();
                    store.LoadFromFile(file);

                    Assert.AreEqual(this._original, store.Graphs.First());
                }
            }
            else
            {
                Assert.Fail("Unsupported file type");
            }
        }

        [TestMethod]
        public void ParsingLangSpecNTriples()
        {
            this.TestLangSpecParsing("langspec.nt");            
        }

        [TestMethod]
        public void ParsingLangSpecTurtle()
        {
            this.TestLangSpecParsing("langspec.ttl");
        }

        [TestMethod]
        public void ParsingLangSpecN3()
        {
            this.TestLangSpecParsing("langspec.n3");
        }

        [TestMethod]
        public void ParsingLangSpecTriG()
        {
            this.TestLangSpecParsing("langspec.trig");
        }

        [TestMethod]
        public void ParsingLangSpecNQuads()
        {
            this.TestLangSpecParsing("langspec.nq");
        }
    }
}