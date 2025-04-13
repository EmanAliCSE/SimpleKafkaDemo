namespace API.middelwares
{
        public class ActionIdMiddleware
        {
            private readonly RequestDelegate _next;

            public ActionIdMiddleware(RequestDelegate next)
            {
                _next = next;
            }

            public async Task InvokeAsync(HttpContext context)
            {
                var actionId = Guid.NewGuid().ToString(); 
                context.Items["ActionId"] = actionId;
            //_logger.LogInformation(" Generated ActionId: {ActionId}", actionId); // debug log

            await _next(context);
            }
        }

}
