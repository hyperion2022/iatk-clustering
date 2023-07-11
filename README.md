# Clustering and Visualisation of 3D Trajectories
Project by Mihai ANCA during the internship at Inria Bordeaux (10.05.2021 - 09.07.2021) under the supervision of Arnaud PROUZEAU.

I was tasked with implementing the k-means algorithm for clustering 2D points and then adapt it to function with 3D trajectories. I have used the DTW algorithm for calculating the distance between two trajectories and adapted the algorithm to its k-means++ optimisation.

This repository contains the project itself, as well as a presentation I have created to explain the DTW algorithm and the report I've submitted at the end of the internship documenting my work (in French).

In order to showcase it, I have used the IATK library to create a first-person demo of what the results could look like when used at a workplace with the help of AR tools. Graphs are contained in a cube and can be controlled dynamically in the environment.

A video of it can be found here: [Démo IATK Entreprise - YouTube](https://www.youtube.com/watch?v=Ba3jaqK19S4)