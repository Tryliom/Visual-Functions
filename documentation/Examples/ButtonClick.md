[Back](../MoreExamples.md)

# Button click
Here we use functions change the text of a button when clicked by the number of times.

Start by adding to your game object or the button the component `FunctionOnAction`, then add the `Evaluate` function to it from the button `Add function`.

Then, add global values named:
- `_button`, set his type to `Component of Game Object`, then move the button to it, click on button `...` and select the type `TextMeshProUGUI` in children
- `_clicks`, set his type to `int` and set the value to 0

![ButtonClickGlobalValues.png](../images/ButtonClickGlobalValues.png)

Then select the `Evaluator` function and set the value to `_clicks += 1; _button.text = "Clicked " + clicks + " times"`.

![ButtonClickEvaluator.png](../images/ButtonClickEvaluator.png)

Then, on the button, add the `OnClick` event to your game object and select the `FunctionOnAction` component, then select the `LaunchFunctions` method.

![ButtonOnClick.png](../images/ButtonOnClick.png)

Try it by clicking on the play button at the right of the `OnAction` event or by starting the game and click on the button.