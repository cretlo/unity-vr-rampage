using UnityEngine;
using System.Collections;

namespace Artngame.TreeGEN
{
    public class SplineMeshExtrusionIvySTUDIO
{
	public class Edge
	{		
		public int[]  vertexIndex = new int[2];		
		public int[]  faceIndex = new int[2];
	}
	
	public static void ExtrudeMesh (Mesh srcMesh, Mesh extrudedMesh, Matrix4x4[] extrusion, bool invertFaces, float growThicknessFactor)
	{
		Edge[] edges = BuildManifoldEdges(srcMesh);
		ExtrudeMesh(srcMesh, extrudedMesh, extrusion, edges, invertFaces,false,null, growThicknessFactor);
	}	
	
	public static void ExtrudeMesh (Mesh srcMesh, Mesh extrudedMesh, Matrix4x4[] extrusion, Edge[] edges, bool invertFaces, bool cap_to_parent, Vector3[] parent_hole, float growThicknessFactor)
	{
		int extrudedVertexCount = edges.Length * 2 * extrusion.Length;
		int triIndicesPerStep = edges.Length * 6;
		int extrudedTriIndexCount = triIndicesPerStep * (extrusion.Length -1);
		
		Vector3[] inputVertices = srcMesh.vertices;

		Vector2[] inputUV = srcMesh.uv;
		int[] inputTriangles = srcMesh.triangles;

		Vector3[] vertices = new Vector3[extrudedVertexCount + srcMesh.vertexCount * 2];
		Vector2[] uvs = new Vector2[vertices.Length];
		int[] triangles = new int[extrudedTriIndexCount + inputTriangles.Length * 2];

		// Build extruded vertices
		int v = 0;
		float current_full_length=0;
		for (int i=0;i<extrusion.Length;i++)
		{
			Matrix4x4 matrix = extrusion[i];
			//float vcoord = (float)i / (extrusion.Length -1);
			
			int count_edges = 0;
			foreach (Edge e in edges)
			{			

					if(parent_hole!=null){
						if(cap_to_parent & (i == extrusion.Length-1)){


							if(count_edges<parent_hole.Length-1){

								vertices[v+0] = parent_hole[count_edges];
								vertices[v+1] = parent_hole[count_edges+1];

							}else{

								vertices[v+0] = parent_hole[count_edges];
								vertices[v+1] = parent_hole[0];

							}
							count_edges++;
							
						}else{
							vertices[v+0] = matrix.MultiplyPoint(inputVertices[e.vertexIndex[0]]*(i*0.2f * growThicknessFactor));
							vertices[v+1] = matrix.MultiplyPoint(inputVertices[e.vertexIndex[1]]*(i*0.2f * growThicknessFactor))*1;
						}
					}else{
							vertices[v+0] = matrix.MultiplyPoint(inputVertices[e.vertexIndex[0]]*(i*0.2f * growThicknessFactor));
							vertices[v+1] = matrix.MultiplyPoint(inputVertices[e.vertexIndex[1]]*(i*0.2f * growThicknessFactor))*1;
					}
				
				v += 2;
			}

				if(i>0){
					current_full_length = current_full_length + Vector3.Distance(vertices[v-2-(2*edges.Length)],vertices[v+0-2]);
				}
		}		



			//CONNECTOR TO BARK - grab profile here !!! - for each profile egde, create extra vertices
			//Matrix4x4 matrix1 = extrusion[extrusion.Length-1];//get last extrusion ? (or first ?)
			//foreach (Edge e in edges)
			//{
				//for testing - MUST get vertices from parent hole area !!!! or section
				//vertices[v+0] = matrix1.MultiplyPoint(inputVertices[e.vertexIndex[0]]*(1*0.2f))+new Vector3(0,-20,0);
				//vertices[v+1] = matrix1.MultiplyPoint(inputVertices[e.vertexIndex[1]]*(1*0.2f))*1+new Vector3(0,-20,0);
			//}
			//OTHER approach, conform last vertices to profile of parent !!!


		
		// Build cap vertices
		// * The bottom mesh we scale along it's negative extrusion direction. This way extruding a half sphere results in a capsule.
		for (int c=0;c<2;c++)
		{
			Matrix4x4 matrix = extrusion[c == 0 ? 0 : extrusion.Length-1];
			int firstCapVertex = c == 0 ? extrudedVertexCount : extrudedVertexCount + inputVertices.Length;
			for (int i=0;i<inputVertices.Length;i++)
			{
				vertices[firstCapVertex + i] = matrix.MultiplyPoint(inputVertices[i]);
				uvs[firstCapVertex + i] = inputUV[i];
			}
		}
		
		// Build extruded triangles
			int v1 = 0;

			//NEW UVs
			bool toggle_uv_mapper = true;
			//bool toggle_uv_mapper2 = true;
			float current_full_length2=0;

		for (int i=0;i<extrusion.Length-1;i++)
		{
			int baseVertexIndex = (edges.Length * 2) * i;
			int nextVertexIndex = (edges.Length * 2) * (i+1);

				//NEW UVs
				//float vcoord = (float)i / (extrusion.Length -1);

					if(toggle_uv_mapper){
						toggle_uv_mapper=false;
					}else{
						toggle_uv_mapper=true;
					}					

			for (int e=0;e<edges.Length;e++)
			{
				int triIndex = i * triIndicesPerStep + e * 6;

				triangles[triIndex + 0] = baseVertexIndex + e * 2;
				triangles[triIndex + 1] = nextVertexIndex  + e * 2;///
				triangles[triIndex + 2] = baseVertexIndex + e * 2 + 1;//
				triangles[triIndex + 3] = nextVertexIndex + e * 2;///
				triangles[triIndex + 4] = nextVertexIndex + e * 2 + 1;
				triangles[triIndex + 5] = baseVertexIndex  + e * 2 + 1;//

					//NEW UVs

					if(i>0){

						uvs[v1+0] = new Vector2 (0, (float)(current_full_length2 / current_full_length) );
						uvs[v1+1] = new Vector2 (1, (float)(current_full_length2 / current_full_length) );

					}else{
						uvs[v1+0] = new Vector2 (0, 0);
						uvs[v1+1] = new Vector2 (1, 0);
					}

					v1 += 2;
			}

				if(i>0){
					current_full_length2 = current_full_length2 + Vector3.Distance(vertices[v1-2-(2*edges.Length)],vertices[v1+0-2]);
				}

		}
		
		// build cap triangles
		int triCount = inputTriangles.Length / 3;
		// Top
		{
			int firstCapVertex = extrudedVertexCount;
			int firstCapTriIndex = extrudedTriIndexCount;
			for (int i=0;i<triCount;i++)
			{
				triangles[i*3 + firstCapTriIndex + 0] = inputTriangles[i * 3 + 1] + firstCapVertex;
				triangles[i*3 + firstCapTriIndex + 1] = inputTriangles[i * 3 + 2] + firstCapVertex;
				triangles[i*3 + firstCapTriIndex + 2] = inputTriangles[i * 3 + 0] + firstCapVertex;
			}
		}
		
		// Bottom
		{
			int firstCapVertex = extrudedVertexCount + inputVertices.Length;
			int firstCapTriIndex = extrudedTriIndexCount + inputTriangles.Length;
			for (int i=0;i<triCount;i++)
			{
				triangles[i*3 + firstCapTriIndex + 0] = inputTriangles[i * 3 + 0] + firstCapVertex;
				triangles[i*3 + firstCapTriIndex + 1] = inputTriangles[i * 3 + 2] + firstCapVertex;
				triangles[i*3 + firstCapTriIndex + 2] = inputTriangles[i * 3 + 1] + firstCapVertex;
			}
		}
		
		if (invertFaces)
		{
			for (int i=0;i<triangles.Length/3;i++)
			{
				int temp = triangles[i*3 + 0];
				triangles[i*3 + 0] = triangles[i*3 + 1];
				triangles[i*3 + 1] = temp;
			}
		}
		
		extrudedMesh.Clear();
		extrudedMesh.name= "extruded";
		extrudedMesh.vertices = vertices;
		extrudedMesh.uv = uvs;
		extrudedMesh.triangles = triangles;
		extrudedMesh.RecalculateNormals();
	}

	/// Builds an array of edges that connect to only one triangle.
	
	public static Edge[] BuildManifoldEdges (Mesh mesh)
	{
		if(mesh !=null){
			// Build a edge list for all unique edges in the mesh
			Edge[] edges = BuildEdges(mesh.vertexCount, mesh.triangles);
			
			// We only want edges that connect to a single triangle
			ArrayList culledEdges = new ArrayList();
			foreach (Edge edge in edges)
			{
				if (edge.faceIndex[0] == edge.faceIndex[1])
				{
					culledEdges.Add(edge);
				}
			}

			return culledEdges.ToArray(typeof(Edge)) as Edge[];
		}else{
			Debug.Log ("Problem");
			return null;
		}



	}

	/// Builds an array of unique edges	
	public static Edge[] BuildEdges(int vertexCount, int[] triangleArray)
	{
		int maxEdgeCount = triangleArray.Length;
		int[] firstEdge = new int[vertexCount + maxEdgeCount];
		int nextEdge = vertexCount;
		int triangleCount = triangleArray.Length / 3;
		
		for (int a = 0; a < vertexCount; a++)
			firstEdge[a] = -1;
			
		
		Edge[] edgeArray = new Edge[maxEdgeCount];
		
		int edgeCount = 0;
		for (int a = 0; a < triangleCount; a++)
		{
			int i1 = triangleArray[a*3 + 2];
			for (int b = 0; b < 3; b++)
			{
				int i2 = triangleArray[a*3 + b];
				if (i1 < i2)
				{
					Edge newEdge = new Edge();
					newEdge.vertexIndex[0] = i1;
					newEdge.vertexIndex[1] = i2;
					newEdge.faceIndex[0] = a;
					newEdge.faceIndex[1] = a;
					edgeArray[edgeCount] = newEdge;
					
					int edgeIndex = firstEdge[i1];
					if (edgeIndex == -1)
					{
						firstEdge[i1] = edgeCount;
					}
					else
					{
						while (true)
						{
							int index = firstEdge[nextEdge + edgeIndex];
							if (index == -1)
							{
								firstEdge[nextEdge + edgeIndex] = edgeCount;
								break;
							}
						
							edgeIndex = index;
						}
					}
			
					firstEdge[nextEdge + edgeCount] = -1;
					edgeCount++;
				}
			
				i1 = i2;
			}
		}
		

		for (int a = 0; a < triangleCount; a++)
		{
			int i1 = triangleArray[a*3+2];
			for (int b = 0; b < 3; b++)
			{
				int i2 = triangleArray[a*3+b];
				if (i1 > i2)
				{
					bool foundEdge = false;
					for (int edgeIndex = firstEdge[i2]; edgeIndex != -1;edgeIndex = firstEdge[nextEdge + edgeIndex])
					{
						Edge edge = edgeArray[edgeIndex];
						if ((edge.vertexIndex[1] == i1) && (edge.faceIndex[0] == edge.faceIndex[1]))
						{
							edgeArray[edgeIndex].faceIndex[1] = a;
							foundEdge = true;
							break;
						}
					}
					
					if (!foundEdge)
					{
						Edge newEdge = new Edge();
						newEdge.vertexIndex[0] = i1;
						newEdge.vertexIndex[1] = i2;
						newEdge.faceIndex[0] = a;
						newEdge.faceIndex[1] = a;
						edgeArray[edgeCount] = newEdge;
						edgeCount++;
					}
				}
				
				i1 = i2;
			}
		}
		
		Edge[] compactedEdges = new Edge[edgeCount];
		for (int e=0;e<edgeCount;e++)
			compactedEdges[e] = edgeArray[e];
		
		return compactedEdges;
	}
}
}