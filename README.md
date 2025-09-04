# F1InputTelemetry

F1InputTelemetry is a lightweight real-time overlay for the F1® games from 2018 to 2025. It captures UDP telemetry data from the game and shows your throttle, brake, clutch, and steering inputs directly on screen, similar to Racelabs Input Telemetry.

## Features

*   **Real-time Input Display:** Instantly visualize your throttle, brake, clutch, and steering inputs.
*   **Telemetry Graph:** A line graph shows your throttle and brake history for quick analysis.
*   **Customizable Overlay:** Adjust the position and scale via `settings.yaml` file.
*   **Auto-Hide:** Optionally, the overlay can automatically hide when not in a session and appear when a session starts.
*   **Spectator Support:** Automatically switches to display the telemetry data of the car you are spectating.

## Installation and Usage

1.  **Download:** Go to the [**Releases page**](https://github.com/4nder123/F1InputTelemetry/releases) and download the latest `F1InputTelemetry.exe` file.
2.  **Run:** Place the `.exe` in a folder of your choice and run it. A `settings.yaml` file will be created in the same directory.
3.  **Play:** The overlay will now appear and display your inputs while you are in a session. You can customize the application by editing the `settings.yaml` file (close and restart the application for changes to take effect).

> ⚠️ **Important:** If you change or have changed the **IP address, port, or send rate** in your game’s telemetry settings, you must also update those same values in the `settings.yaml` file to ensure the overlay works correctly.

## Configuration

The application is configured by editing the `settings.yaml` file that is generated on the first run.

| Parameter | Description | Default |
| :--- | :--- | :--- |
| `IPAddress` | The IP address to listen on for UDP telemetry data. | `127.0.0.1` |
| `Port` | The UDP port to listen on. | `20777` |
| `SendRate` | The telemetry send rate from the game (e.g., 20 Hz). | `20` |
| `WindowX` | The horizontal screen coordinate for the center of the overlay. | `960` |
| `WindowY` | The vertical screen coordinate for the center of the overlay. | `815` |
| `WindowScale` | A multiplier to scale the size of the overlay. `1.0` is 100%. | `1.0` |
| `AutoHide` | If `true`, the overlay only appears when an in-game session is active. | `false` |
| `ShowClutch` | If `true`, the clutch input bar is displayed. | `true` |

## License

This project is licensed under the MIT License. See the `LICENSE.txt` file for details.
