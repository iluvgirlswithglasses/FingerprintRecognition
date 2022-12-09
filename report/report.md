---
title: "Fingerprint Recognition"
author: "Lưu Nam Đạt"
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

$$A(M) = \dfrac{\sum M_{ij}}{Size(M)}$$

$$S = Std(M) = \sqrt{ \dfrac{ \sum((M_{ij} - A(M))^2) }{Size(M)} }$$

$$\delta = \dfrac{\sqrt{S_{0} \times (M_{ij} - A(M))^2}}{S} $$

$$M_{ij} = \begin{cases}A_{0} - \delta,& \text{if } M_{ij} \geq A(M) \\ A_{0} + \delta, & \text{if } M_{ij} < A(M)\end{cases}$$

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

## Verdict: Match

![1](./figures/m-0-r-1.jpg){width=33%} ![9](./figures/m-0-r-9.jpg){width=33%} ![10](./figures/m-0-r-10.jpg){width=33%}

![1](./figures/m-0-s-1.png){width=33%} ![9](./figures/m-0-s-9.png){width=33%} ![10](./figures/m-0-s-10.png){width=33%}

```
Comparing 1 and 9: MMScore = 0.02355706937779257 [True]
Comparing 1 and 10: MMScore = 0.03147042916615236 [True]
```

\pagebreak

## MMScore's Calculation

For each span:

$$\text{Ridges Mismatch Score} = \dfrac{|A-B|}{max(A, b)}$$

\pagebreak

## Verdict: Match

![11](./figures/m-1-r-11.bmp){width=33%} ![13](./figures/m-1-r-13.bmp){width=33%} ![14](./figures/m-1-r-14.bmp){width=33%}

![11](./figures/m-1-s-11.png){width=33%} ![13](./figures/m-1-s-13.png){width=33%} ![14](./figures/m-1-s-14.png){width=33%}

```
Comparing 11 and 13: MMScore = 0.06850775829491 [True]
Comparing 11 and 14: MMScore = 0.057465974475778385 [True]
```

\pagebreak

## Verdict: Mismatch

![0](./figures/mm-0-r-0.jpg){width=33%} ![4](./figures/mm-0-r-4.jpg){width=33%}

![0](./figures/mm-0-s-0.png){width=33%} ![4](./figures/mm-0-s-4.png){width=33%}

```
Comparing 0 and 4: Ridge MMScore = 0.11450558858066591 [False]
```

\pagebreak

## Verdict: Mismatch

![16](./figures/mm-1-r-16.bmp){width=33%} ![21](./figures/mm-1-r-21.bmp){width=33%}

![16](./figures/mm-1-s-16.png){width=33%} ![21](./figures/mm-1-s-21.png){width=33%}

```
Comparing 16 and 21:
CA = (268, 155), CB = (259, 183);
Ridge MMScore = 0.2133995812504585; 
Singu MMScore = 0.5; 
MMScore = 0.7133995812504585 [False]
```
