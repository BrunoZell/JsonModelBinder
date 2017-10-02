# JsonModelBinder
This library provides an explicit json model binder to allow json serialized parts in a multipart-formdata request of a .Net-Core controller action.

Now you can, for example, have a file upload action with additional data about the upload as a json structure. Then your controller upload action would look like this:

```c#
[HttpPost]
public IActionResult Upload([ModelBinder(BinderType = typeof(JsonModelBinder))] JsonModel model, IFormFile file) {
    // model is now deserialized from the formdata-part named 'model'
    // file represents the file sent in the formdata-part named 'file'
    return Ok();
}
```

An valid request for the above action would be a POST with `content-type: multipart/form-data` with two parts specified: `model` (the json string according to the model type) and `file` (the form-data file). The model binder will take care of the deserialization process and will provide you with a concrete model class as specified in the controller action signature.

The `JsonModelBinder` will use the defined `MvcJsonOptions` from startup (`.AddMvc().AddJsonOptions(...)`). If these options are not available the default Json.Net settings will be used to deserialize the model.
