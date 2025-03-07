﻿using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Pathfinder.Pathfinding.Algorithms;
using Pathfinder.GUI;
using Avalonia.Platform.Storage;

namespace Pathfinder;

/// <summary>
/// Sisältää UI-tapahtumien käsittelijät
/// </summary>
public partial class MainWindow
{
    /// <summary>
    /// Event handler start-napin klikkausta varten
    /// </summary>
    /// <param name="sender">Lähettävä olio</param>
    /// <param name="e">EventArgs-data</param>
    private void StartButton_Click(object? sender, RoutedEventArgs e)
    {
        StartVisualization(VisualizationMode.SinglePath);
    }

    /// <summary>
    /// Event handler select file napin klikkausta varten
    /// </summary>
    /// <param name="sender">Lähettävä olio</param>
    /// <param name="e">EventArgs-data</param>
    private async void SelectFileButton_Click(object? sender, RoutedEventArgs e)
    {
        var topLevel = GetTopLevel(this);
        if (topLevel is null)
            return;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Map File",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
            new FilePickerFileType("Map Files")
            {
                Patterns = new[] { "*.map" }
            },
            new FilePickerFileType("All Files")
            {
                Patterns = new[] { "*.*" }
            }
        }
        });

        if (files is null || files.Count == 0)
        {
            return;
        }

        var file = files[0];

        MapTextBox.Text = file.Path.AbsolutePath;
    }

    /// <summary>
    /// Event handler random-napin klikkausta varten
    /// </summary>
    /// <param name="sender">Lähettävä olio</param>
    /// <param name="e">EventArgs-data</param>
    private void RandomPathsButton_Click(object? sender, RoutedEventArgs e)
    {
        StartVisualization(VisualizationMode.RandomPathBenchmark);
    }

    /// <summary>
    /// Event handler kuvan hiiren painallukselle
    /// </summary>
    /// <param name="sender">Lähettävä olio</param>
    /// <param name="e">PointerEventArgs-data</param>
    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(VisualizationImage).Properties.IsLeftButtonPressed)
        {
            _startPosition = e.GetPosition(VisualizationImage);
            StartTextBox.Text = $"{(int)_startPosition.X},{(int)_startPosition.Y}";
        }
    }

    /// <summary>
    /// Event handler kuvan hiiren liikkeelle
    /// </summary>
    /// <param name="sender">Lähettävä olio</param>
    /// <param name="e">PointerEventArgs-data</param>
    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (e.GetCurrentPoint(VisualizationImage).Properties.IsLeftButtonPressed)
        {
            Point currentPosition = e.GetPosition(VisualizationImage);
            UpdateLine(_startPosition, currentPosition);
        }
    }

    /// <summary>
    /// Event handler kuvan hiiren vapautukselle
    /// </summary>
    /// <param name="sender">Lähettävä olio</param>
    /// <param name="e">PointerEventArgs-data</param>
    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (e.InitialPressMouseButton == MouseButton.Left)
        {
            var goalPosition = e.GetPosition(VisualizationImage);
            GoalTextBox.Text = $"{(int)goalPosition.X},{(int)goalPosition.Y}";
        }
    }

    /// <summary>
    /// Event handler sliderin arvon muuttumiselle
    /// </summary>
    /// <param name="sender">Lähettävä olio</param>
    /// <param name="e">RangeBaseValueChangedEventArgs-data</param>
    private void Slider_ValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        if (_stepDelay is null) { return; }

        var stepDelay = Math.Pow(StepDelaySlider.Value, 4);
        var stepDelayTimeSpan = TimeSpan.FromMicroseconds(stepDelay);
        _stepDelay.SetTargetStepDelay(stepDelayTimeSpan);
    }

    /// <summary>
    /// Event handler karttatiedoston tekstilaatikon muuttumiselle
    /// </summary>
    /// <param name="sender">Lähettävä olio</param>
    /// <param name="e">TextChangedEventArgs-data</param>
    private void MapTextBox_TextChanged(object? sender, TextChangedEventArgs e)
    {
        var mapPath = MapTextBox.Text;
        if (mapPath is null)
        {
            return;
        }

        InitMap(mapPath);
    }

    /// <summary>
    /// Event handler algoritmi-valikon muuttumiselle
    /// </summary>
    /// <param name="sender">Lähettävä olio</param>
    /// <param name="e">SelectionChangedEventArgs-data</param>
    private void AlgorithmComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (AllowDiagonalCheckBox is null) { return; }

        var selected = GetSelectedAlgorithm();
        if (selected is JumpPointSearch)
        {
            AllowDiagonalCheckBox.IsChecked = true;
            AllowDiagonalCheckBox.IsEnabled = false;
        }
        else
        {
            AllowDiagonalCheckBox.IsEnabled = true;
        }
    }
}
