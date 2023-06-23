using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using Random = UnityEngine.Random;
using System.Globalization;

public class TCluster
{
    public Vector3[] Centroid;
    public List<Vector3[]> Trajectories = new List<Vector3[]>();
}

public class Trajectory : MonoBehaviour
{
    [SerializeField] uint k = 8;
    [SerializeField] string rpath = "Assets/Resources/flight_Paris_processed.csv";
    [SerializeField] string wpath = "Assets/Resources/trajectories.csv";
    [SerializeField] string col_x = "longitude";
    [SerializeField] string col_y = "latitude";
    [SerializeField] string col_z = "baro_altitude";
    [SerializeField] string col_trajID = "callsign";

    void Start()
    {
        CultureInfo.CurrentCulture = new CultureInfo("en-US", false);
        kMeansTClustering(k, rpath, wpath, col_x, col_y, col_z, col_trajID);
    }

    public void kMeansTClustering(uint k, string ReadPath, string WritePath, string col_x, string col_y, string col_z, string col_trajID)
    {   
        // TRAJECTOIRES
        List<Vector3[]> Trajectories = ReadTrajectories(ReadPath, col_x, col_y, col_z, col_trajID);

        // CENTROIDS
        List<int> L = new List<int>();
        int index = Random.Range(0, Trajectories.Count);
        List<TCluster> Clusters = new List<TCluster>();
        for (uint i = 0; i < k; i++)
        {   
            TCluster C = new TCluster();
            while (L.Contains(index))
                index = Random.Range(0, Trajectories.Count);
            L.Add(index);
            C.Centroid = Trajectories[index];
            Clusters.Add(C);
        }

        int cpt, nb_iter = 0;
        do
        {
            // REINIT. DES CLUSTERS
            foreach (TCluster C in Clusters)
            {
                C.Trajectories.Clear();
            }

            cpt = 0;

            // AJOUT DE TRAJ. DANS LES CLUSTERS COURANTS
            foreach (Vector3[] T in Trajectories)
            {   
                TCluster min_clust = Clusters[0];
                float min_dist = float.MaxValue;
                foreach (TCluster C in Clusters)
                {
                    float dist = DTWDistance(C.Centroid, T);
                    if (dist < min_dist)
                    {
                        min_clust = C;
                        min_dist = dist;
                    }
                }

                Clusters[Clusters.IndexOf(min_clust)].Trajectories.Add(T);
            }

            // CALCUL DES NOUVEAUX CENTROIDS
            foreach (TCluster C in Clusters)
            {   
                Vector3[] NewCentroid = RandomWeightedCentroid(C.Centroid, Trajectories);
                if (NewCentroid.Equals(C.Centroid)) cpt++;
                else C.Centroid = NewCentroid;
            }

            nb_iter++;
        } while (cpt != Clusters.Count && nb_iter < 1000);

        WriteClusters(Clusters, WritePath);
    }

    // Renvoie un nouveau centroid avec une probabilité non-uniforme
    // basée sur la distance entre le centroid courant et les trajectoires
    public Vector3[] RandomWeightedCentroid(Vector3[] Centroid, List<Vector3[]> Trajectories)
    {
        float[] Distances = new float[Trajectories.Count];
        float DistanceSum = 0;

        for (int i = 0; i < Distances.Length; i++)
        {
            Distances[i] = DTWDistance(Centroid, Trajectories[i]);
            DistanceSum += Distances[i];
        }
        
        float rnd = Random.Range(0, DistanceSum);

        for (int i = 0; i < Distances.Length; i++)
        {
            if (rnd < Distances[i])
            {
                return Trajectories[i];
            }
            rnd -= Distances[i];
        }

        Debug.Log("Should never get here!");
        return null;
    }

    // Renvoie la distance entre 2 trajectoire (algorithme DTW)
    public float DTWDistance(Vector3[] A, Vector3[] B)
    {
        float[,] DTW = new float[A.Length, B.Length];

        DTW[0, 0] = Vector3.Distance(A[0], B[0]);
        for (uint i = 1; i < A.Length; i++)
        {
            float cost = Vector3.Distance(A[i], B[0]);
            DTW[i, 0] = cost + DTW[i - 1, 0];
        }

        for (uint j = 1; j < B.Length; j++)
        {
            float cost = Vector3.Distance(A[0], B[j]);
            DTW[0, j] = cost + DTW[0, j - 1];
        }
        
        for (uint i = 1; i < A.Length; i++)
        {
            for (uint j = 1; j < B.Length; j++)
            {
                float cost = Vector3.Distance(A[i], B[j]);
                DTW[i, j] = cost + Mathf.Min(Mathf.Min(DTW[i - 1, j], DTW[i, j - 1]), DTW[i - 1, j - 1]);
            }
        }

        return DTW[A.Length - 1, B.Length - 1];
    }

    // Renvoie les trajectoires lues à partir d'un fichier .csv
    // avec les noms des colonnes passés en paramètre
    public List<Vector3[]> ReadTrajectories(string path, string col_x, string col_y, string col_z, string col_trajID)
    {   
        List<List<Vector3>> Trajectories = new List<List<Vector3>>();
        StreamReader Reader = new StreamReader(path);
        
        List<Vector3> CurrentTrajectory = new List<Vector3>();
        
        string line = Reader.ReadLine();
        string[] bits = line.Split(',');
        
        int index_x = 0, index_y = 0, index_z = 0, index_trajID = 0;
        for (int i = 0; i < bits.Length; i++)
        {
            if (bits[i] == col_x)
            {
                index_x = i;
            }
            else if (bits[i] == col_y)
            {
                index_y = i;
            }
            else if (bits[i] == col_z)
            {
                index_z = i;
            }
            else if (bits[i] == col_trajID)
            {
                index_trajID = i;
            }
        }

        line = Reader.ReadLine();
        bits = line.Split(',');

        string currentID = String.Copy(bits[index_trajID]);
        float x = float.Parse(bits[index_x]);
        float y = float.Parse(bits[index_y]);
        float z = float.Parse(bits[index_z]);
        CurrentTrajectory.Add(new Vector3(x, y, z));
        
        line = Reader.ReadLine();
        while (line != null)
        {
            bits = line.Split(',');
            string trajID = bits[index_trajID];
            
            if (trajID != currentID)
            {
                Trajectories.Add(CurrentTrajectory);
                currentID = String.Copy(trajID);
                CurrentTrajectory = new List<Vector3>();
            }
            
            x = float.Parse(bits[index_x]);
            y = float.Parse(bits[index_y]);
            z = float.Parse(bits[index_z]);
            CurrentTrajectory.Add(new Vector3(x, y, z));

            line = Reader.ReadLine();
            if (line == null)
            {
                Trajectories.Add(CurrentTrajectory);
                CurrentTrajectory = new List<Vector3>();
            }
        }
        
        List<Vector3[]> TrajectoryList = new List<Vector3[]>();
        Vector3[] PointArray;

        foreach (List<Vector3> T in Trajectories)
        {
            PointArray = new Vector3[T.Count];
            uint index = 0;
            foreach (Vector3 P in T)
            {
                PointArray[index] = P;
                index++;
            }
            TrajectoryList.Add(PointArray);
        }
        
        Reader.Close();
        Debug.Log("Done Reading!");
        return TrajectoryList;
    }

    // Ecrit les clusters dans un fichier.csv
    // avec le format "x,y,z,trajectory,cluster"
    public void WriteClusters(List<TCluster> Clusters, string filename)
    {
        string path = filename;
        StreamWriter Writer = new StreamWriter(path, false);
        Writer.WriteLine("x,y,z,trajectory,cluster");
        int nb_trajectory = 0, nb_cluster = 0;
        foreach (TCluster c in Clusters)
        {
            foreach(Vector3[] t in c.Trajectories)
            {
                foreach (Vector3 p in t)
                {
                    Writer.WriteLine(p.x.ToString() + "," + p.y.ToString() + "," + p.z.ToString() + "," + nb_trajectory.ToString() + "," + nb_cluster.ToString());
                }
                nb_trajectory++;
            }
            nb_cluster++; 
        }
        
        Writer.Close();
        Debug.Log("Done Writing!");
    }

}