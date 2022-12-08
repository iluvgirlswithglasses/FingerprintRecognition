---
title: "Fingerprint Recognition"
author: "iluvgirlswithglasses"
date: "Dec-5-2022"
geometry: margin=2cm
output: 
  pdf_document: 
    latex_engine: xelatex
fontsize: 12pt
---

![Source Image](./figures/0-src.jpg)

\pagebreak

# 1. Preprocessing

## 1.1. Normalization

![Normalization](./figures/1-norm.png){height=40%}

$$S = Std(M) = \sqrt{ \dfrac{ \sum((M_{ij} - Avg(M))^2) }{Size(M)} }$$

$$\delta = \dfrac{\sqrt{S_{0} \times (M_{ij} - Avg(M))^2}}{S} $$

$$M_{ij} = \begin{cases}Avg_{0} - \delta,& \text{if } M_{ij} \geq Avg(M) \\ Avg_{0} + \delta, & \text{if } M_{ij} < Avg(M)\end{cases}$$

\pagebreak

## 1.2. Segmentation

![Normalization](./figures/1-norm.png){width=33%} ![Segment Mask](./figures/2-mask.png){width=33%} ![Segmented Image](./figures/3-segment-img.png){width=33%}

A block $K$ in image $M$ is background if: 

$$Std(K) \leq Std(M) \times \text{threshold}$$

Dilation, followed by Erosion are performed to unify the blocks.

Then, erosion followed by dilation are performed to exclude insignificant blocks.

\pagebreak

## 1.3. Orientation

![Ox](./figures/4-sobel-x.png){width=33%} ![Oy](./figures/5-sobel-y.png){width=33%} ![Visual](./figures/6-orient-visualized.png){width=33%}

*(The third image is only used for visualization only. It does not take part in any computing process.)*

Sobel Operator is used for detect gradient along $Ox$ and $Oy$.

Then, for each pixel in image, we can calculate the gradient angle:

$$\theta = \tan^{-1}\dfrac{|\overrightarrow{Gx}|}{|\overrightarrow{Gy}|}$$

Then we can calculate the gradient angle for each block.

\pagebreak

## 1.4. Ridges' Frequency

![Calculating Ridges' Frequency](./figures/freq.png){height=40%}

$\theta$ is calculated in the *Orientation* step.

\pagebreak

## 1.5. Gabor filter

![src](./figures/1-norm.png){width=33%} ![Filter](./figures/gabor-filter.png){width=33%} ![Result](./figures/7-gabor.png){width=33%}

The size of the filter is determined by *ridge frequency*.

The Rotation of the filter is determined by *block orientation*.

![Another Example](./figures/gabor-filter-2.png){height=40%}

\pagebreak

## 1.6. Skeletonization

![Skeletonization](./figures/8-sket.png){width=40%}

\pagebreak

## 1.7. Singularities and Keypoints

![Detect Singularities](./figures/corepoints.png){height=25%}

![Singularities and Keypoints](./figures/9-singularities.png){height=40%}

\pagebreak

# 2. Comparing

## Cases of Mismatch


