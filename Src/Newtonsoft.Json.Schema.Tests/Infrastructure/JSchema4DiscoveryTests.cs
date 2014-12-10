﻿#region License
// Copyright (c) Newtonsoft. All Rights Reserved.
// License: https://raw.github.com/JamesNK/Newtonsoft.Json.Schema/master/LICENSE.md
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema.V4;
using Newtonsoft.Json.Schema.V4.Infrastructure;
using NUnit.Framework;

namespace Newtonsoft.Json.Schema.Tests.Infrastructure
{
    [TestFixture]
    public class JSchema4DiscoveryTests : TestFixtureBase
    {
        [Test]
        public void SimpleTest()
        {
            JSchema4 prop = new JSchema4();
            JSchema4 root = new JSchema4
            {
                Properties = new Dictionary<string, JSchema4>
                {
                    { "prop1", prop },
                    { "prop2", prop }
                }
            };

            JSchema4Discovery discovery = new JSchema4Discovery();
            discovery.Discover(root, null);

            Assert.AreEqual(2, discovery.KnownSchemas.Count);
            Assert.AreEqual(root, discovery.KnownSchemas[0].Schema);
            Assert.AreEqual(new Uri("#", UriKind.RelativeOrAbsolute), discovery.KnownSchemas[0].Id);
            Assert.AreEqual(prop, discovery.KnownSchemas[1].Schema);
            Assert.AreEqual(new Uri("#/properties/prop1", UriKind.RelativeOrAbsolute), discovery.KnownSchemas[1].Id);
        }

        [Test]
        public void SimpleTest_RootId()
        {
            JSchema4 prop = new JSchema4();
            JSchema4 root = new JSchema4
            {
                Id = new Uri("http://localhost/"),
                Properties = new Dictionary<string, JSchema4>
                {
                    { "prop1", prop },
                    { "prop2", prop }
                }
            };

            JSchema4Discovery discovery = new JSchema4Discovery();
            discovery.Discover(root, null);

            Assert.AreEqual(2, discovery.KnownSchemas.Count);
            Assert.AreEqual(root, discovery.KnownSchemas[0].Schema);
            Assert.AreEqual(new Uri("http://localhost/#", UriKind.RelativeOrAbsolute), discovery.KnownSchemas[0].Id);
            Assert.AreEqual(prop, discovery.KnownSchemas[1].Schema);
            Assert.AreEqual(new Uri("http://localhost/#/properties/prop1", UriKind.RelativeOrAbsolute), discovery.KnownSchemas[1].Id);
        }

        [Test]
        public void RootId_NestedId()
        {
            JSchema4 prop1 = new JSchema4
            {
                Id = new Uri("test.json/", UriKind.RelativeOrAbsolute),
                Items = new List<JSchema4>
                {
                    new JSchema4(),
                    new JSchema4
                    {
                        Id = new Uri("#fragmentItem2", UriKind.RelativeOrAbsolute),
                        Items = new List<JSchema4>
                        {
                            new JSchema4(),
                            new JSchema4 { Id = new Uri("#fragmentItem2Item2", UriKind.RelativeOrAbsolute) },
                            new JSchema4 { Id = new Uri("file.json", UriKind.RelativeOrAbsolute) },
                            new JSchema4 { Id = new Uri("/file1.json", UriKind.RelativeOrAbsolute) }
                        }
                    }
                }
            };
            JSchema4 prop2 = new JSchema4
            {
                Id = new Uri("#fragment", UriKind.RelativeOrAbsolute),
                Not = new JSchema4()
            };
            JSchema4 root = new JSchema4
            {
                Id = new Uri("http://localhost/", UriKind.RelativeOrAbsolute),
                Properties = new Dictionary<string, JSchema4>
                {
                    { "prop1", prop1 },
                    { "prop2", prop2 }
                },
                ExtensionData =
                {
                    {
                        "definitions",
                        new JObject
                        {
                            { "def1", new JSchema4() },
                            { "def2", new JSchema4 { Id = new Uri("def2.json", UriKind.RelativeOrAbsolute) } },
                            {
                                "defn",
                                new JArray
                                {
                                    new JValue(5),
                                    new JSchema4()
                                }
                            }
                        }
                    }
                }
            };

            JSchema4Discovery discovery = new JSchema4Discovery();
            discovery.Discover(root, null);

            int i = 0;

            Assert.AreEqual(new Uri("http://localhost/#", UriKind.RelativeOrAbsolute), discovery.KnownSchemas[i].Id);
            Assert.AreEqual(root, discovery.KnownSchemas[i++].Schema);

            Assert.AreEqual(new Uri("http://localhost/#/definitions/def1", UriKind.RelativeOrAbsolute), discovery.KnownSchemas[i].Id);
            Assert.AreEqual((JSchema4)root.ExtensionData["definitions"]["def1"], discovery.KnownSchemas[i++].Schema);

            Assert.AreEqual(new Uri("http://localhost/def2.json", UriKind.RelativeOrAbsolute), discovery.KnownSchemas[i].Id);
            Assert.AreEqual((JSchema4)root.ExtensionData["definitions"]["def2"], discovery.KnownSchemas[i++].Schema);

            Assert.AreEqual(new Uri("http://localhost/#/definitions/defn/1", UriKind.RelativeOrAbsolute), discovery.KnownSchemas[i].Id);
            Assert.AreEqual((JSchema4)root.ExtensionData["definitions"]["defn"][1], discovery.KnownSchemas[i++].Schema);

            Assert.AreEqual(new Uri("http://localhost/test.json/", UriKind.RelativeOrAbsolute), discovery.KnownSchemas[i].Id);
            Assert.AreEqual(root.Properties["prop1"], discovery.KnownSchemas[i++].Schema);

            Assert.AreEqual(new Uri("http://localhost/test.json/#/items/0", UriKind.RelativeOrAbsolute), discovery.KnownSchemas[i].Id);
            Assert.AreEqual(root.Properties["prop1"].Items[0], discovery.KnownSchemas[i++].Schema);

            Assert.AreEqual(new Uri("http://localhost/test.json/#fragmentItem2", UriKind.RelativeOrAbsolute), discovery.KnownSchemas[i].Id);
            Assert.AreEqual(root.Properties["prop1"].Items[1], discovery.KnownSchemas[i++].Schema);

            Assert.AreEqual(new Uri("http://localhost/test.json/#fragmentItem2/items/0", UriKind.RelativeOrAbsolute), discovery.KnownSchemas[i].Id);
            Assert.AreEqual(root.Properties["prop1"].Items[1].Items[0], discovery.KnownSchemas[i++].Schema);

            Assert.AreEqual(new Uri("http://localhost/test.json/#fragmentItem2Item2", UriKind.RelativeOrAbsolute), discovery.KnownSchemas[i].Id);
            Assert.AreEqual(root.Properties["prop1"].Items[1].Items[1], discovery.KnownSchemas[i++].Schema);

            Assert.AreEqual(new Uri("http://localhost/test.json/file.json", UriKind.RelativeOrAbsolute), discovery.KnownSchemas[i].Id);
            Assert.AreEqual(root.Properties["prop1"].Items[1].Items[2], discovery.KnownSchemas[i++].Schema);

            Assert.AreEqual(new Uri("http://localhost/file1.json", UriKind.RelativeOrAbsolute), discovery.KnownSchemas[i].Id);
            Assert.AreEqual(root.Properties["prop1"].Items[1].Items[3], discovery.KnownSchemas[i++].Schema);

            Assert.AreEqual(new Uri("http://localhost/#fragment", UriKind.RelativeOrAbsolute), discovery.KnownSchemas[i].Id);
            Assert.AreEqual(root.Properties["prop2"], discovery.KnownSchemas[i++].Schema);

            Assert.AreEqual(new Uri("http://localhost/#fragment/not", UriKind.RelativeOrAbsolute), discovery.KnownSchemas[i].Id);
            Assert.AreEqual(root.Properties["prop2"].Not, discovery.KnownSchemas[i++].Schema);
        }

        [Test]
        public void Draft4Example()
        {
            JSchema4 schema1 = new JSchema4
            {
                Id = new Uri("#foo", UriKind.RelativeOrAbsolute),
                Title = "schema1"
            };
            JSchema4 schema2 = new JSchema4
            {
                Id = new Uri("otherschema.json", UriKind.RelativeOrAbsolute),
                Title = "schema2",
                ExtensionData =
                {
                    {
                        "nested",
                        new JSchema4
                        {
                            Title = "nested",
                            Id = new Uri("#bar", UriKind.RelativeOrAbsolute)
                        }
                    },
                    {
                        "alsonested",
                        new JSchema4
                        {
                            Title = "alsonested",
                            Id = new Uri("t/inner.json#a", UriKind.RelativeOrAbsolute),
                            ExtensionData =
                            {
                                {
                                    "nestedmore",
                                    new JSchema4 { Title = "nestedmore" }
                                }
                            }
                        }
                    }
                }
            };
            JSchema4 schema3 = new JSchema4
            {
                Title = "schema3",
                Id = new Uri("some://where.else/completely#", UriKind.RelativeOrAbsolute)
            };

            JSchema4 root = new JSchema4
            {
                Id = new Uri("http://x.y.z/rootschema.json#", UriKind.RelativeOrAbsolute),
                ExtensionData =
                {
                    { "schema1", schema1 },
                    { "schema2", schema2 },
                    { "schema3", schema3 }
                }
            };

            JSchema4Discovery discovery = new JSchema4Discovery();
            discovery.Discover(root, null);

            Assert.AreEqual(7, discovery.KnownSchemas.Count);
            Assert.AreEqual("http://x.y.z/rootschema.json#", discovery.KnownSchemas[0].Id.ToString());
            Assert.AreEqual("http://x.y.z/rootschema.json#foo", discovery.KnownSchemas[1].Id.ToString());
            Assert.AreEqual("http://x.y.z/otherschema.json", discovery.KnownSchemas[2].Id.ToString());
            Assert.AreEqual("http://x.y.z/otherschema.json#bar", discovery.KnownSchemas[3].Id.ToString());
            Assert.AreEqual("http://x.y.z/t/inner.json#a", discovery.KnownSchemas[4].Id.ToString());
            Assert.AreEqual("http://x.y.z/t/inner.json#/nestedmore", discovery.KnownSchemas[5].Id.ToString());
            Assert.AreEqual("some://where.else/completely#", discovery.KnownSchemas[6].Id.ToString());
        }
    }
}