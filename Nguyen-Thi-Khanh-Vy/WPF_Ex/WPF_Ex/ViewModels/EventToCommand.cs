using System;
using System.Windows;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace WPF_Ex.Behaviors
{
    /// <summary>
    /// Behavior này chuyển đổi một sự kiện thành một command.
    /// Nếu PassEventArgsToCommand được đặt là true, thì đối tượng event (EventArgs) sẽ được truyền làm CommandParameter,
    /// trừ khi CommandParameter đã được set.
    /// </summary>
    public class EventToCommand : TriggerAction<DependencyObject>
    {
        #region Dependency Properties

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(EventToCommand), new PropertyMetadata(null));

        /// <summary>
        /// Command sẽ được gọi khi sự kiện được kích hoạt.
        /// </summary>
        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register("CommandParameter", typeof(object), typeof(EventToCommand), new PropertyMetadata(null));

        /// <summary>
        /// Tham số sẽ được truyền cho Command khi nó được thực thi.
        /// Nếu để trống và PassEventArgsToCommand = true thì đối tượng event (EventArgs) sẽ được truyền vào.
        /// </summary>
        public object CommandParameter
        {
            get { return GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        public static readonly DependencyProperty PassEventArgsToCommandProperty =
            DependencyProperty.Register("PassEventArgsToCommand", typeof(bool), typeof(EventToCommand), new PropertyMetadata(false));

        /// <summary>
        /// Nếu true, đối tượng sự kiện (EventArgs) sẽ được truyền cho Command nếu CommandParameter chưa được set.
        /// </summary>
        public bool PassEventArgsToCommand
        {
            get { return (bool)GetValue(PassEventArgsToCommandProperty); }
            set { SetValue(PassEventArgsToCommandProperty, value); }
        }

        #endregion

        protected override void Invoke(object parameter)
        {
            if (Command == null)
            {
                return;
            }

            object commandParameter = CommandParameter;

            // Nếu CommandParameter chưa được set và PassEventArgsToCommand = true, sử dụng đối tượng event (parameter)
            if (commandParameter == null && PassEventArgsToCommand)
            {
                commandParameter = parameter;
            }

            if (Command.CanExecute(commandParameter))
            {
                Command.Execute(commandParameter);
            }
        }
    }
}
